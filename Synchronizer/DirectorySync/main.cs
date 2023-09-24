using SynchronizationLibrary;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;

namespace Main
{
    class Hello
    {
        //static Timer timer;
        static bool continueWriting = true;
        static string sourceDir;
        static public string destinationDir;
       static  StreamWriter streamWriter;
        static public Sync syncronize;
        
        static void Main(string[] args)
        {

            PrintUsage();
            
            sourceDir = args[0].ToString();
            destinationDir = args[1].ToString(); 
            int timeIntervalSync = Convert.ToInt32(args[2]);
            string logFile = args[3].ToString();
            if (File.Exists(logFile))
            {
                
                try
                {

                    //streamWriter = new StreamWriter(logFile, true);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception: " + e.ToString() + "arose. It is not possible to open log file for writting.");
                }
            }
            else 
            {
                Console.WriteLine("File for logging does not exist.");
                //return false;
            }
            
            syncronize = new Sync(sourceDir, destinationDir);

            syncronize.Log = (string message) => 
            { 
                Console.WriteLine(message); 
                File.AppendAllText(logFile, message);

            };
            System.Timers.Timer timer1 = new System.Timers.Timer(timeIntervalSync);
            ElapsedEventHandler elapsedEventHandler = new ElapsedEventHandler(updateFolders);
            timer1.Elapsed += elapsedEventHandler;
            timer1.Start();

            Thread.Sleep(100000);
            timer1.Stop();
            timer1.Dispose();
        }
        static void updateFolders(object sender, ElapsedEventArgs e)
        {
            syncronize.Start();
        }
        public static void PrintUsage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("SyncronizationLibrary: <source directory tree> <destination directory tree> <time interval to update> <Log file>");
        }

    }
}
