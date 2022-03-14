using System;
using dc.assignment.primenumbers.utils.logger;

namespace dc.assignment.primenumbers
{
    class Program
    {

        public static KLogger? logger;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Missing arguments!");
                return;
            }
            // logger viewer node runner
            else if (args.Length == 1 && args[0].Equals("logviewer"))
            {
                // application logger
                Program.logger = new KLogger(true);
            }
            // AppNode runner
            else if (args.Length == 3 && args[0].Equals("appnode"))
            {
                // application logger
                Program.logger = new KLogger(false);

                string ip = args[1];
                int port = int.Parse(args[2]);

                AppNode node = new AppNode(ip, port);
                Program.logger.log(node.id, node.getAddress(), "AppNode created.");
            }
            else
            {
                Console.WriteLine("Error: Invalid arguments!");
                return;
            }
        }
    }
}