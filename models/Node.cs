namespace dc.assignment.primenumbers.models
{
    class Node
    {
        public AppNodeType type { get; set; }
        public string address { get; set; } = "";
        public bool isAlive { get; set; }
    }
}