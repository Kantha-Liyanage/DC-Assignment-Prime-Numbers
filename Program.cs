using dc.assignment.primenumbers.utils.logger;

namespace dc.assignment.primenumbers{
    class Program{

        public static KLogger? logger;

        static void Main(string[] args)
        {
            if(args.Length != 2){
                Console.WriteLine("Invalid input.");
                return;
            }

            Program.logger = new KLogger();
            string ip = args[0];
            int port = int.Parse(args[1]);

            AppNode node = new AppNode(ip,port);
            Program.logger.log(node.id, node.getAddress(), "AppNode created.");
        }
    }
}