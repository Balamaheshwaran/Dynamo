﻿//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Greg;
using Greg.Requests;
using Greg.Responses;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     A thin wrapper on the Greg rest client for performing IO with
    ///     the Package Manager
    /// </summary>
    public class PackageManagerClient
    {

        /// <summary>
        /// Indicates whether we should look for login information
        /// </summary>
        public static bool DEBUG_MODE = true;

        #region Properties

        /// <summary>
        ///     Controller property
        /// </summary>
        /// <value>
        ///     Reference to the main DynamoController
        /// </value>
        private readonly DynamoController Controller;

        /// <summary>
        ///     Client property
        /// </summary>
        /// <value>
        ///     The client for the Package Manager
        /// </value>
        public Client Client { get; internal set; }

        /// <summary>
        ///     IsLoggedIn property
        /// </summary>
        /// <value>
        ///     Specifies whether the user is logged in or not.
        /// </value>
        public bool IsLoggedIn { get; internal set; }

        /// <summary>
        ///     Worker property
        /// </summary>
        /// <value>
        ///     Helps to do asynchronous calls to the server
        /// </value>
        public BackgroundWorker Worker { get; internal set; }

        /// <summary>
        ///     LoadedPackageHeaders property
        /// </summary>
        /// <value>
        ///     Tells which package headers are currently loaded
        /// </value>
        public Dictionary<FunctionDefinition, PackageHeader> LoadedPackageHeaders { get; internal set; }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="controller"> Reference to to the DynamoController object for the app </param>
        public PackageManagerClient(DynamoController controller)
        {
            Controller = controller;

            //IAuthProvider provider = new RevitOxygenProvider(Autodesk.Revit.AdWebServicesBase.GetInstance());
           
            LoadedPackageHeaders = new Dictionary<FunctionDefinition, PackageHeader>();
            Client = new Client(null, "http://54.225.215.247");
            Worker = new BackgroundWorker();

            IsLoggedIn = false;
        }

        /// <summary>
        ///     Asynchronously pull the package headers from the server and update search
        /// </summary>
        public void RefreshAvailable()
        {
            ThreadStart start = () =>
                {
                    HeaderCollectionDownload req = HeaderCollectionDownload.ByEngine("dynamo");

                    try
                    {
                        ResponseWithContentBody<List<PackageHeader>> response =
                            Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(req);
                        if (response.success)
                        {
                            dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                                {
                                    foreach (PackageHeader header in response.content)
                                    {
                                        dynSettings.Controller.SearchViewModel.Add(header);
                                    }
                                }));
                        }
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Controller.DynamoViewModel.Log("Failed to refresh available nodes from server.")));
                    }
                };
            new Thread(start).Start();
        }

        /// <summary>
        ///     Create a PackageUpload object from the given data
        /// </summary>
        /// <param name="funDef"> The function definition for the user-defined node </param>
        /// <param name="version"> The version, specified in X.Y.Z form</param>
        /// <param name="description"> A description of the user-defined node </param>
        /// <param name="keywords"> Keywords to describe the user-defined node </param>
        /// <param name="license"> A license string (e.g. "MIT") </param>
        /// <param name="group"> The "group" for the package (e.g. DynamoTutorial) </param>
        /// <returns> Returns null if it fails to get the xmlDoc, otherwise a valid PackageUpload </returns>
        public PackageUpload GetPackageUpload(FunctionDefinition funDef, string version, string description,
                                              List<string> keywords, string license, string group, List<string> files, List<PackageDependency> deps )
        {
            // var group = ((FuncWorkspace) funDef.Workspace).Category;
            string name = funDef.Workspace.Name;
            var contents = "";
            string engineVersion = "0.1.0"; //nope

            string engineMetadata = "";

            var pkg = PackageUpload.MakeDynamoPackage(name, version, description, keywords, license,
                                                                contents,
                                                                engineVersion, engineMetadata, files, deps);
            return pkg;
        }

        internal List<Search.SearchElements.PackageManagerSearchElement> Search(string search, int MaxNumSearchResults)
        {
            var nv = new Greg.Requests.Search(search);
            var pkgResponse = Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
            return pkgResponse.content.GetRange(0,Math.Min(MaxNumSearchResults, pkgResponse.content.Count())).Select((header) => new PackageManagerSearchElement(header)).ToList();
        }

        /// <summary>
        ///     Create a PackageVersionUpload object from the given data
        /// </summary>
        /// <param name="funDef"> The function definition for the user-defined node </param>
        /// <param name="packageHeader"> The PackageHeader object </param>
        /// <param name="version"> The version, specified in X.Y.Z form</param>
        /// <param name="description"> A description of the user-defined node </param>
        /// <param name="keywords"> Keywords to describe the user-defined node </param>
        /// <param name="license"> A license string (e.g. "MIT") </param>
        /// <param name="group"> The "group" for the package (e.g. DynamoTutorial) </param>
        /// <returns>Returns null if it fails to get the xmlDoc, otherwise a valid PackageVersionUpload  </returns>
        public PackageVersionUpload GetPackageVersionUpload(FunctionDefinition funDef, PackageHeader packageHeader,
                                                            string version,
                                                            string description, List<string> keywords, string license,
                                                            string group, List<string> files, List<PackageDependency> deps)
        {
            // var group = ((FuncWorkspace) funDef.Workspace).Category;
            string name = funDef.Workspace.Name;
            var xml = dynWorkspaceModel.GetXmlDocFromWorkspace(funDef.Workspace, false);
            if (xml == null) return null;
            var contents = xml.OuterXml;
            string engineVersion = "0.1.0"; //nope
            string engineMetadata = "FunctionDefinitionGuid:" + funDef.FunctionId.ToString();

            var pkg = new PackageVersionUpload(name, version, description, keywords, contents, "dynamo",
                                                engineVersion,
                                                engineMetadata, files, deps );
            return pkg;
        }

        /// <summary>
        ///     Attempt to upload PackageUpload
        /// </summary>
        /// <param name="packageUpload"> The PackageUpload object - the payload </param>
        /// <param name="funDef">
        ///     The function definition for the user-defined node - necessary to
        ///     update the LoadedPackageHeaders array on load
        /// </param>
        public void Publish(PackageUpload packageUpload, FunctionDefinition funDef)
        {
            ThreadStart start = () =>
                {
                    try
                    {
                        ResponseWithContentBody<PackageHeader> ret =
                            Client.ExecuteAndDeserializeWithContent<PackageHeader>(packageUpload);
                        dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                            {
                                dynSettings.Controller.DynamoViewModel.Log("Message form server: " + ret.message);
                                LoadedPackageHeaders.Add(funDef, ret.content);
                                SavePackageHeader(ret.content);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Controller.DynamoViewModel.Log("Failed to publish package.")));
                    }
                };
            new Thread(start).Start();
        }

        /// <summary>
        ///     Attempt to upload PackageVersionUpload
        /// </summary>
        /// <param name="pkgVersUpload"> The PackageUpload object - the payload </param>
        public void Publish(PackageVersionUpload pkgVersUpload)
        {
            ThreadStart start = () =>
                {
                    try
                    {
                        ResponseWithContentBody<PackageHeader> ret =
                            Client.ExecuteAndDeserializeWithContent<PackageHeader>(pkgVersUpload);
                        dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                            {
                                dynSettings.Controller.DynamoViewModel.Log(ret.message);
                                SavePackageHeader(ret.content);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Controller.DynamoViewModel.Log("Failed to publish package.")));
                    }
                };
            new Thread(start).Start();
        }

        /// <summary>
        ///     Serialize and save a PackageHeader to the "Packages" directory
        /// </summary>
        /// <param name="pkgHeader"> The PackageHeader object </param>
        public void SavePackageHeader(PackageHeader pkgHeader)
        {
            try
            {
                var m2 = new JsonSerializer();
                string s = m2.Serialize(pkgHeader);

                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "packages");

                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                // now save it
                string path = Path.Combine(pluginsPath, pkgHeader.name + ".json");
                File.WriteAllText(path, s);
            }
            catch
            {
                dynSettings.Bench.Dispatcher.BeginInvoke(
                    (Action)
                    (() => dynSettings.Controller.DynamoViewModel.Log(
                        "Failed to write package header information, won't be under source control.")));
            }
        }

        /// <summary>
        ///     Asynchronously download a specific user-defined node from the server
        /// </summary>
        /// <param name="id"> The id that uniquely defines the package, usually obtained from a PackageHeader </param>
        /// <param name="version"> A version name for the download </param>
        /// <param name="callback"> Delegate to execute upon receiving the package </param>
        public void Download(string id, string version, Action<Guid> callback)
        {
            ThreadStart start = () =>
                {   
                    // download the package
                    var m = new HeaderDownload(id);
                    ResponseWithContentBody<PackageHeader> p = Client.ExecuteAndDeserializeWithContent<PackageHeader>(m);

                    
                    
                };
            new Thread(start).Start();
        }


        /// <summary>
        ///     Attempts to load a PackageHeader from the Packages directory, if successful, stores the PackageHeader
        /// </summary>
        /// <param name="funcDef"> The FunctionDefinition to which the loaded user-defined node is to be assigned </param>
        /// <param name="name">
        ///     The name of the package, necessary for looking it up in Packages. Note that
        ///     two package version cannot exist side by side.
        /// </param>
        public void LoadPackageHeader(FunctionDefinition funcDef, string name)
        {
            try
            {
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "packages");

                // find the file matching the expected name
                string[] files = Directory.GetFiles(pluginsPath, name + ".json");

                if (files.Length == 1) // There can only be one!
                {
                    // open and deserialize to a PackageHeader object
                    // this is a bit hacky looking, but does the job
                    var proxyResponse = new RestResponse();
                    proxyResponse.Content = File.ReadAllText(files[0]);
                    var jsonDes = new JsonDeserializer();
                    var packageHeader = jsonDes.Deserialize<PackageHeader>(proxyResponse);
                    dynSettings.Controller.DynamoViewModel.Log("Loading package control information for " + name + " from packages");
                    LoadedPackageHeaders.Add(funcDef, packageHeader);
                }
            }
            catch (Exception ex)
            {
                dynSettings.Controller.DynamoViewModel.Log("Failed to open the package header information.");
                dynSettings.Controller.DynamoViewModel.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
            }
        } 

        internal void Download(DynamoPackageDownload dynamoPackageDownload)
        {
            throw new NotImplementedException();
        }
    }
}