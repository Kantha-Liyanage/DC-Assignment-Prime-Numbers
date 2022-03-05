using dc.assignment.primenumbers.utils.logger;

namespace dc.assignment.primenumbers{
    class Program{

        public static KLogger? logger;

        static void Main(string[] args)
        {
            Program.logger = new KLogger();

            AppNode node = new AppNode("127.0.0.1",5050);
            Program.logger.log("Node ID:" + node.id, node.toString());
        }

    }
}