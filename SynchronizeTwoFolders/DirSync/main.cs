using System;
using System.IO;
using System.Timers;

using SynchronizationLibrary;

namespace Main
{
    class Program
    {
        static string sourceDir;
        static string destinationDir;
        static Sync synchronize;

        static void Main(string[] args)
        {
            // Print usage instructions
            PrintUsage();

            if (args.Length != 4)
            {
                return;
            }

            sourceDir = args[0];
            destinationDir = args[1];
            int timeIntervalSync;

            if (!int.TryParse(args[2], out timeIntervalSync))
            {
                Console.WriteLine("Invalid time interval value.");
                return;
            }

            string logFile = args[3];

            // Check if the log file exists or create a new one
            if (!File.Exists(logFile))
            {
                Console.WriteLine("File for logging does not exist. Creating a new one.");
                File.Create(logFile).Close(); // Close the file stream after creation
                if (File.Exists(logFile))
                {
                    Console.WriteLine(logFile, " Log file created.");
                    File.AppendAllText(logFile, " Log file created.");
                }
                else
                {
                    Console.WriteLine("Not possible to create the log file.");
                    return;
                }
            }

            // Call the constructor for the Sync class and assign where traces will be printed
            InitializeSynchronization(sourceDir, destinationDir, logFile);

            using (System.Timers.Timer updateRepeatedly = new System.Timers.Timer(timeIntervalSync))
            {
                updateRepeatedly.Elapsed += UpdateFolders;
                updateRepeatedly.Start();
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                updateRepeatedly.Stop();
                updateRepeatedly.Dispose();
            }
        }

        static void InitializeSynchronization(string source, string dest, string logFile)
        {
            synchronize = new Sync(source, dest);

            synchronize.Log = (string message) =>
            {
                Console.WriteLine(message);
                File.AppendAllText(logFile, message);
            };
        }

        // Perform synchronization request
        static void UpdateFolders(object sender, ElapsedEventArgs e)
        {
            synchronize.Start();
        }

        // Print usage instructions 
        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("SynchronizationLibrary: <source directory tree> <destination directory tree> <time interval to update> <Log file>");
            Console.WriteLine("Do not forget to enclose file paths in quotes!");
            Console.WriteLine("To stop synchronization, please press Enter.");
        }
    }
}
