﻿using System;
using System.IO;
using System.Reflection;
using Dynamo.Nodes;

namespace Dynamo
{
    public class DynamoLogger
    {
        private static DynamoLogger instance;

        public TextWriter Writer { get; set; }

        private string _logPath;
        public string LogPath 
        {
            get { return _logPath; }
        }

        /// <summary>
        /// The singelton instance.
        /// </summary>
        public static DynamoLogger Instance
        {
            get
            {
                if(instance == null)
                    instance = new DynamoLogger();
                return instance;
            }
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        private DynamoLogger()
        {
            
        }

        /// <summary>
        /// Log the time and the supplied message.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            if (Writer != null)
            {
                try
                {
                    Writer.WriteLine(string.Format("{0} : {1}", DateTime.Now, message));
                }
                catch
                {
                    // likely caught if the writer is closed
                }
            }
                
        }

        public void Log(dynNodeModel node)
        {
            string exp = node.PrintExpression();
            Log("> " + exp);
        }

        public void Log(FScheme.Expression expression)
        {
            Log(FScheme.printExpression("\t", expression));
        }

        /// <summary>
        /// Begin logging.
        /// </summary>
        public void StartLogging()
        {
            //create log files in a directory 
            //with the executing assembly
            string log_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dynamo_logs");
            if (!Directory.Exists(log_dir))
            {
                Directory.CreateDirectory(log_dir);
            }

            _logPath = Path.Combine(log_dir, string.Format("dynamoLog_{0}.txt", Guid.NewGuid().ToString()));

            Writer = new StreamWriter(_logPath);
            Writer.WriteLine("Dynamo log started " + DateTime.Now.ToString());
        }

        /// <summary>
        /// Finish logging.
        /// </summary>
        public void FinishLogging()
        {
            if (Writer != null)
            {
                try
                {
                    this.Log("Goodbye");
                    Writer.Close();
                }
                catch
                {
                }
            }
        }
    }
}
