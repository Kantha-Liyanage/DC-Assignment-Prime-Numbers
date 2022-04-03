using System;

namespace dc.assignment.primenumbers.models
{
    class Node
    {
        public AppNodeType type { get; set; }
        public string name { get; set; } = "";
        public Int64 id { get; set; }
        public string address { get; set; } = "";
        public bool isAlive { get; set; }
    }
}