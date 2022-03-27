namespace dc.assignment.primenumbers.models
{
    class Node
    {
        public AppNodeType type { get; set; }
        public string name { get; set; }
        public string address { get; set; } = "";
        public ServiceMeta meta { get; set; }
        public ServiceCheck check { get; set; }

        public Node()
        {
            this.meta = new ServiceMeta();
            this.check = new ServiceCheck();
            this.check.deregisterCriticalServiceAfter = "90m";
            this.check.interval = "5s";
            this.check.timeout = "5s";
        }
    }

    class ServiceCheck
    {
        public string deregisterCriticalServiceAfter { get; set; }
        public string[] arg { get; set; }
        public string interval { get; set; }
        public string timeout { get; set; }
    }

    class ServiceMeta
    {
        public AppNodeType nodeType { get; set; }
    }
}