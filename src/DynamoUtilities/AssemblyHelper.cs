﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Dynamo.Utilities
{
    public static class AssemblyHelper
    {
        /// <summary>
        /// Attempts to resolve an assembly from the dll directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string folderPath = String.Empty;
            folderPath = String.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location)?
                Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath):
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath  + @"\dll", new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath))
                return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        public static Version GetDynamoVersion()
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetName().Version;
        }

        public static Assembly LoadLibG()
        {
            var libG = Assembly.LoadFrom(GetLibGPath());
            return libG;
        }

        public static string GetLibGPath()
        {
            string dll_dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\dll";
            string libGPath = Path.Combine(dll_dir, "LibGNet.dll");
            return libGPath;
        }

        /// <summary>
        /// Load an assembly from a byte array.
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <returns></returns>
        public static Assembly LoadAssemblyFromStream(string assemblyPath)
        {
            var assemblyBytes = File.ReadAllBytes(assemblyPath);
            var pdbPath = Path.Combine(Path.GetDirectoryName(assemblyPath),
                Path.GetFileNameWithoutExtension(assemblyPath) + ".pdb");

            Assembly assembly = null;

            if (File.Exists(pdbPath))
            {
                var pdbBytes = File.ReadAllBytes(pdbPath);
                assembly = Assembly.Load(assemblyBytes, pdbBytes);
            }
            else
            {
                assembly = Assembly.Load(assemblyBytes);
            }
            return assembly;
        }

        /// <summary>
        /// Create an instance of an object from DynamoCore.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static object CreateInstanceByNameFromCore(string typeName)
        {
            string basePath = String.Empty;
            basePath = String.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location)
                ? Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)
                : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var corePath = Path.Combine(basePath, "DynamoCore.dll");
            var coreAssembly = AssemblyHelper.LoadAssemblyFromStream(corePath);

            var objType = coreAssembly.GetType(typeName);
            var obj = Activator.CreateInstance(objType);

            return obj;
        }

        /// <summary>
        /// Count the number of DynamoCore assemblies that are loaded and write to debug.
        /// </summary>
        public static void DebugDynamoCoreInstances()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            try
            {
                var cores = assemblies.Where(x => x.FullName.Split(',')[0] == "DynamoCore");
                Debug.WriteLine(string.Format("There are {0} DynamoCore assemblies loaded.", cores.Count()));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Assembly resolution callback. Resolves assemblies, by loading them from 
        /// byte arrays. Allows dynamic reloading of assemblies.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly ResolveAssemblyDynamically(object sender, ResolveEventArgs args)
        {
            var name = args.Name.Split(',')[0];

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly found = assemblies.FirstOrDefault(x => x.FullName.Split(',')[0] == name);

            if (found != null)
            {
                var version = new Version(args.Name.Split(',')[1].Split('=')[1]);
                if (found.GetName().Version >= version)
                {
                    return found;
                }
            }

            //The assembly has not already been loaded. Attempt to load the assembly
            //looking first in the executing assembly's directory, then in the /dll sub-directory.
            Assembly assembly = null;
            try
            {
                //get the folder to load dlls from
                var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var dllPath = Path.Combine(folder, name + ".dll");
                var dllSubPath = Path.Combine(folder + @"\dll", name + ".dll");

                if (File.Exists(dllPath))
                {
                    assembly = LoadAssemblyFromStream(dllPath);
                }
                else if (File.Exists(dllSubPath))
                {
                    assembly = LoadAssemblyFromStream(dllSubPath);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return null;
            }

            DebugDynamoCoreInstances();
            Debug.WriteLine("Resolved assembly:" + args.Name);
            return assembly;
        }

        public static void LoadAssembliesInDirectoryIfNewer(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new Exception("The specified directory does not exist.");
            }

            var di = new DirectoryInfo(path);
            var dlls = di.GetFiles("*.dll");

            foreach (var dll in dlls)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(dll.FullName);

                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    Assembly found = assemblies.FirstOrDefault(x => x.FullName.Split(',')[0] == fileName);

                    if (found != null)
                    {
                        var foundVersion = found.GetName().Version;

                        var dllVersion = FileVersionInfo.GetVersionInfo(dll.FullName).FileVersion == null?
                            new Version() : 
                            new Version(FileVersionInfo.GetVersionInfo(dll.FullName).FileVersion);

                        if (dllVersion > foundVersion)
                        {
                            LoadAssemblyFromStream(dll.FullName);
                        }
                    }
                    else
                    {
                        LoadAssemblyFromStream(dll.FullName);
                    }
                }
                catch
                {
                    continue;
                }
                
            }
        }
    }
}
