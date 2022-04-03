using System;
using dc.assignment.primenumbers.utils.logger;

namespace dc.assignment.primenumbers
{
    class Program
    {

        public static KLogger? logger;

        static void Main(string[] args)
        {
            string[] inputArgs;
            string[] test = { "appnode", "127.0.0.1", "5050" };

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
            else if (inputArgs.Length == 1 && inputArgs[0].Equals("logviewer"))
            {
                // application logger
                Program.logger = new KLogger(true);
            }
            // AppNode runner
            else if (inputArgs.Length == 3 && inputArgs[0].Equals("appnode"))
            {
                // application logger
                Program.logger = new KLogger(false);

                string ip = inputArgs[1];
                int port = int.Parse(inputArgs[2]);

                AppNode node = new AppNode(ip, port);
                Program.logger.log(node.id, node.address, "AppNode created.");
            }
            else
            {
                Console.WriteLine("Error: Invalid arguments!");
                return;
            }
        }
    }
}