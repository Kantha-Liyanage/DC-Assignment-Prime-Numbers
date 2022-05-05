using System;
using dc.assignment.primenumbers.models;
using dc.assignment.primenumbers.utils.log;

namespace dc.assignment.primenumbers
{
    class Program
    {
        private static KLogger? appLogger;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Missing arguments!");
                return;
            }
            // logger viewer node runner
            else if (args.Length == 1 && args[0].Equals("logger"))
            {
                // application logger
                Program.appLogger = new KLogger(true);
                Console.Title = "Logger";
            }
            // numbers file service
            else if (args.Length == 1 && args[0].Equals("nfs"))
            {
                NumbersFileHandler numbersDatFileHandler = new NumbersFileHandler("data/numbers.txt", "data/output.txt");
                Console.Title = "Number File Service";
            }
            // AppNode runner
            else if (args.Length == 3 && args[0].Equals("appnode"))
            {
                // application logger
                Program.appLogger = new KLogger(false);

                string ip = args[1];
                int port = int.Parse(args[2]);

                AppNode node = new AppNode(ip, port);
            }
            else
            {
                Console.WriteLine("Error: Invalid arguments!");
                return;
            }
        }

        // application logging
        public static void log(Int64 nodeId, string nodeName, string message)
        {
            // remote logging
            Program.appLogger.log(nodeId, nodeName, message);

            // console
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Log: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ForegroundColor = defaultColor;
        }
    }
}