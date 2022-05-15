using System;

namespace dc.assignment.primenumbers.models
{
    public class Node
    {
        public AppNodeType type { get; set; }
        public string name { get; set; } = "";
        public Int64 id { get; set; }
        public string address { get; set; } = "";
        public string ipAddress { get; set; }
        public int port { get; set; }
        public int fromNumber { get; set; }
        public int toNumber { get; set; }

        public string getNodeDisplayName()
        {
            return "NodeType: " + this.type + " | ID: " + this.id + " | Address: " + this.address;
        }
    }

    public enum AppNodeType
    {
        Master,
        Proposer,
        Acceptor,
        Learner,
        Initial // not assigned yet
    }
}