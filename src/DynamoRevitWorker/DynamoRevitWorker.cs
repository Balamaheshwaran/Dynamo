﻿using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using RevitServices.Elements;
using String = System.String;
using Rectangle = System.Drawing.Rectangle;

namespace DynamoRevitWorker
{

    [Serializable]
    public class Worker
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

            //updater = DynamoRevitStarterApp.updater;
            //env = DynamoRevitStarterApp.env;

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

            RevitServices.Threading.IdlePromise.ExecuteOnIdleAsync(delegate
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

                internalRevitData.Application.ViewActivating += Application_ViewActivating;
            });
        }

        /// <summary>
        /// Callback on Revit view activation. Addins are not available in some views in Revit, notably perspective views.
        /// This will present a warning that Dynamo is not available to run and disable the run button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_ViewActivating(object sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
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
