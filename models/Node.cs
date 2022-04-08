using System;

namespace dc.assignment.primenumbers.models
{
    public class Node
    {
        public AppNodeType type { get; set; }
        public string name { get; set; } = "";
        public Int64 id { get; set; }
        public string address { get; set; } = "";
        public string ipAddress;
        public int port;
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