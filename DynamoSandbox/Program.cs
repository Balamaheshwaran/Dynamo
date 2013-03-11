﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using System.Windows;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.IO;

using Dynamo;
using Dynamo.Utilities;

using Autodesk.ASM;

namespace DynamoSandbox
{
    class Program
    {
        static DynamoController dynamoController;
        static TextWriter tw;
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string logPath = Path.Combine(tempPath, "dynamoLog.txt");

                tw = new StreamWriter(logPath);
                tw.WriteLine("Dynamo log started " + DateTime.Now.ToString());
                dynSettings.Writer = tw;

                SplashScreen splashScreen = null;
                splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), "splash.png");

                Autodesk.ASM.State.Start();

                dynamoController = new DynamoController(splashScreen);
                var bench = dynamoController.Bench;
                bench.ShowDialog();

                Autodesk.ASM.State.Stop();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
