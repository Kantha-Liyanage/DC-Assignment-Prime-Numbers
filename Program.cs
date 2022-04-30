﻿using System;
using dc.assignment.primenumbers.models;
using dc.assignment.primenumbers.utils.log;

namespace dc.assignment.primenumbers
{
    class Program
    {
        private static KLogger? appLogger;

        static void Main(string[] args)
        {
            string[] inputArgs;
            string[] test = { "appnode", "127.0.0.1", "5051" };

            if (args.Length > 0)
            {
                inputArgs = args;
            }
            else
            {
                inputArgs = test;
            }

            if (inputArgs.Length == 0)
            {
                Console.WriteLine("Error: Missing arguments!");
                return;
            }
            // logger viewer node runner
            else if (inputArgs.Length == 1 && inputArgs[0].Equals("logger"))
            {
                // application logger
                Program.appLogger = new KLogger(true);
            }
            // numbers file service
            else if (inputArgs.Length == 1 && inputArgs[0].Equals("nfs"))
            {
                NumbersFileHandler numbersDatFileHandler = new NumbersFileHandler("data/numbers.txt", "data/output.txt");
            }
            // AppNode runner
            else if (inputArgs.Length == 3 && inputArgs[0].Equals("appnode"))
            {
                // application logger
                Program.appLogger = new KLogger(false);

                string ip = inputArgs[1];
                int port = int.Parse(inputArgs[2]);

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