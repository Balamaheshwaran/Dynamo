﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Dynamo;
using Dynamo.Applications;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using DynamoRevitStarter.Properties;
using RevitServices.Elements;
using RevitServices.Transactions;
using Dynamo.Utilities;
using IdlePromise = RevitServices.Threading.IdlePromise;
using MessageBox = System.Windows.MessageBox;
using Rectangle = System.Drawing.Rectangle;

namespace DynamoRevitStarter
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitStarterApp : IExternalApplication
    {
        public static RevitServicesUpdater updater;
        public static ExecutionEnvironment env;

        public Result OnStartup(UIControlledApplication application)
        {
            SetupDynamoButton(application);

            RevitServices.Threading.IdlePromise.RegisterIdle(application);
            updater = new RevitServicesUpdater(application.ControlledApplication);
            TransactionManager.SetupManager(new DebugTransactionStrategy());
            env = new ExecutionEnvironment();

            return Result.Succeeded;
        }

        private static void SetupDynamoButton(UIControlledApplication application)
        {
            //TAF load english_us TODO add a way to localize
            var res = Resource_en_us.ResourceManager;
            // Create new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(res.GetString("App_Description"));

            var assemblyName = string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location)
                ? new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath
                : Assembly.GetExecutingAssembly().Location;

            //Create a push button in the ribbon panel 
            var pushButton = ribbonPanel.AddItem(new PushButtonData("DynamoDS",
                res.GetString("App_Name"), assemblyName,
                "DynamoRevitStarter.DynamoRevitStarterCommand")) as
                PushButton;

            Bitmap dynamoIcon = Resources.logo_square_32x32;

            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                dynamoIcon.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitStarterCommand : IExternalCommand
    {
/*
        //https://code.google.com/p/revitpythonshell/wiki/FeaturedScriptLoadplugin
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssemblyDynamically;

            Debug.WriteLine("Creating Dynamo AppDomain.");
            AssemblyHelper.DynamoDomain = AppDomain.CreateDomain("Dynamo");

            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPath = Path.Combine(basePath, "DynamoRevitDS.dll");
            var assembly = AssemblyHelper.LoadAssemblyFromStream(assemblyPath);

            if (assembly == null)
            {
                return Result.Failed;
            }

            //create an instance of the DynamoRevit external command object
            //using reflection
            var type = assembly.GetType("Dynamo.Applications.DynamoRevit");
            var dynRevit = Activator.CreateInstance(type);

            //set some fields on the instance of the command
            var updaterField = type.GetField("updater");
            var envField = type.GetField("env");
            updaterField.SetValue(dynRevit, DynamoRevitStarterApp.updater);
            envField.SetValue(dynRevit, DynamoRevitStarterApp.env);

            //execute the command
            var method = type.GetMethod("Execute");
            method.Invoke(dynRevit, new object[] {commandData, message, elements});

            return Result.Succeeded;
        }
      * */

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssemblyDynamically;

            //Add an assembly load step for the System.Windows.Interactivity assembly
            //Revit owns a version of this as well. Adding our step here prevents a duplicative
            //load of the dll at a later time.
            var assLoc = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(assLoc))
            {
                assLoc = @"C:\Program Files\Autodesk\Revit Architecture 2014\";
            }
            var interactivityPath = Path.Combine(Path.GetDirectoryName(assLoc), "System.Windows.Interactivity.dll");
            var interactivityAss = Assembly.LoadFrom(interactivityPath);

            //When a user double-clicks the Dynamo icon, we need to make
            //sure that we don't create another instance of Dynamo.
            /*if (DynamoWorker.isRunning)
            {
                Debug.WriteLine("Dynamo is already running.");
                if (dynamoView != null)
                {
                    dynamoView.Focus();
                }
                return Result.Succeeded;
            }*/

            try
            {
                Debug.WriteLine("Creating Dynamo AppDomain.");
                var domainSetup = new AppDomainSetup {PrivateBinPath = string.Empty};
                domainSetup.PrivateBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                domainSetup.ApplicationBase = domainSetup.PrivateBinPath;

                var dynamoDomain = AppDomain.CreateDomain("Dynamo", null, domainSetup);
                dynamoDomain.AssemblyResolve += AssemblyHelper.ResolveAssemblyDynamically;

                var remoteWorker = (DynamoWorker)dynamoDomain.CreateInstanceAndUnwrap(
                    Assembly.GetExecutingAssembly().FullName,
                    "DynamoRevitStarter.DynamoWorker");
                remoteWorker.internalRevitData = revit;
                remoteWorker.DoDynamo();
            }
            catch (Exception ex)
            {
                DynamoWorker.isRunning = false;
                MessageBox.Show(ex.ToString());

                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
                DynamoLogger.Instance.Log("Dynamo log ended " + DateTime.Now.ToString());

                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }

    [Serializable]
    public class DynamoWorker
    {
        public ExternalCommandData internalRevitData;
        public RevitServicesUpdater updater;
        public ExecutionEnvironment env;
        public static bool isRunning = false;
        public static double? dynamoViewX = null;
        public static double? dynamoViewY = null;
        public static double? dynamoViewWidth = null;
        public static double? dynamoViewHeight = null;
        private bool handledCrash = false;
        private static DynamoView dynamoView;

        public void DoDynamo()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssemblyDynamically;
            dynRevitSettings.Doc = internalRevitData.Application.ActiveUIDocument;

            updater = DynamoRevitStarterApp.updater;
            env = DynamoRevitStarterApp.env;

            isRunning = true;

            #region default level

            Level defaultLevel = null;
            var fecLevel = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fecLevel.OfClass(typeof(Level));
            defaultLevel = fecLevel.ToElements()[0] as Level;

            #endregion

            dynRevitSettings.Revit = internalRevitData.Application;
            dynRevitSettings.Doc = internalRevitData.Application.ActiveUIDocument;
            dynRevitSettings.DefaultLevel = defaultLevel;

            //TODO: has to be changed when we handle multiple docs
            //DynamoRevitApp.Updater.DocumentToWatch = m_doc.Document;
            updater.DocumentToWatch = dynRevitSettings.Doc.Document;

            IdlePromise.ExecuteOnIdleAsync(delegate
            {
                //get window handle
                IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                var r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
                string context = r.Replace(internalRevitData.Application.Application.VersionName, "");

                //they changed the application version name conventions for vasari
                //it no longer has a version year so we can't compare it to other versions
                //TODO:come up with a more stable way to test for Vasari beta 3
                if (context == "Vasari")
                    context = "Vasari 2014";


                //dynamoController = new DynamoController_Revit(DynamoRevitApp.env, DynamoRevitApp.Updater, typeof(DynamoRevitViewModel), context);
                var dynamoController = new DynamoController_Revit(env, updater, typeof(DynamoRevitViewModel), context);

                dynamoView = new DynamoView { DataContext = dynamoController.DynamoViewModel };
                dynamoController.UIDispatcher = dynamoView.Dispatcher;

                //set window handle and show dynamo
                new WindowInteropHelper(dynamoView).Owner = mwHandle;

                handledCrash = false;

                dynamoView.WindowStartupLocation = WindowStartupLocation.Manual;

                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                dynamoView.Left = dynamoViewX ?? bounds.X;
                dynamoView.Top = dynamoViewY ?? bounds.Y;
                dynamoView.Width = dynamoViewWidth ?? 1000.0;
                dynamoView.Height = dynamoViewHeight ?? 800.0;

                dynamoView.Show();

                dynamoView.Dispatcher.UnhandledException -= DispatcherOnUnhandledException;
                dynamoView.Dispatcher.UnhandledException += DispatcherOnUnhandledException;
                dynamoView.Closing += dynamoView_Closing;
                dynamoView.Closed += dynamoView_Closed;

                //revit.Application.ViewActivated += new EventHandler<Autodesk.Revit.UI.Events.ViewActivatedEventArgs>(Application_ViewActivated);
                internalRevitData.Application.ViewActivating += Application_ViewActivating;
            });
        }

        /// <summary>
        /// Callback on Revit view activation. Addins are not available in some views in Revit, notably perspective views.
        /// This will present a warning that Dynamo is not available to run and disable the run button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_ViewActivating(object sender, ViewActivatingEventArgs e)
        {
            var view = e.NewActiveView as View3D;

            if (view != null
                && view.IsPerspective
                && dynSettings.Controller.Context != Context.VASARI_2013
                && dynSettings.Controller.Context != Context.VASARI_2014)
            {
                DynamoLogger.Instance.LogWarning(
                    "Dynamo is not available in a perspective view. Please switch to another view to Run.",
                    WarningLevel.Moderate);
                dynSettings.Controller.DynamoViewModel.RunEnabled = false;
            }
            else
            {
                //alert the user of the new active view and enable the run button
                DynamoLogger.Instance.LogWarning(String.Format("Active view is now {0}", e.NewActiveView.Name), WarningLevel.Mild);
                dynSettings.Controller.DynamoViewModel.RunEnabled = true;
            }
        }

        /// <summary>
        /// A method to deal with unhandle exceptions.  Executes right before Revit crashes.
        /// Dynamo is still valid at this time, but further work may cause corruption.  Here, 
        /// we run the ExitCommand, allowing the user to save all of their work.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Info about the exception</param>
        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;

            // only handle a single crash per Dynamo sesh, this should be reset in the initial command
            if (handledCrash)
            {
                return;
            }

            handledCrash = true;

            var exceptionMessage = args.Exception.Message;

            try
            {
                DynamoLogger.Instance.Log("Dynamo Unhandled Exception");
                DynamoLogger.Instance.Log(exceptionMessage);
            }
            catch
            {

            }

            try
            {
                dynSettings.Controller.OnRequestsCrashPrompt(this, new CrashPromptArgs(args.Exception.Message + "\n\n" + args.Exception.StackTrace));
                dynSettings.Controller.DynamoViewModel.Exit(false); // don't allow cancellation
            }
            catch
            {

            }
            finally
            {
                args.Handled = true;
            }

        }

        /// <summary>
        /// Executes right before Dynamo closes, gives you the chance to cache whatever you might want.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closing(object sender, EventArgs e)
        {
            // cache the size of the window for later reloading
            dynamoViewX = dynamoView.Left;
            dynamoViewY = dynamoView.Top;
            dynamoViewWidth = dynamoView.ActualWidth;
            dynamoViewHeight = dynamoView.ActualHeight;
            IdlePromise.ClearPromises();
            IdlePromise.Shutdown();

            updater.UnRegisterAllChangeHooks();
        }

        /// <summary>
        /// Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closed(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyHelper.ResolveAssemblyDynamically;
            //AppDomain.Unload(AssemblyHelper.DynamoDomain);

            dynamoView = null;
            isRunning = false;
        }
    }
}
