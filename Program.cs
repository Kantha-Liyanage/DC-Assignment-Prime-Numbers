using dc.assignment.primenumbers.utils.logger;

namespace dc.assignment.primenumbers{
    class Program{

        public static KLogger? logger;

        static void Main(string[] args)
        {
            // application logger
            Program.logger = new KLogger();

            /* only needed for the demo
            if(args.Length != 2){
                Console.WriteLine("Invalid input.");
                return;
            }
            string ip = args[0];
            int port = int.Parse(args[1]);
            */

            AppNode node = new AppNode("127.0.0.1",5050);
            Program.logger.log(node.id, node.getAddress(), "AppNode created.");
        }
    }
}