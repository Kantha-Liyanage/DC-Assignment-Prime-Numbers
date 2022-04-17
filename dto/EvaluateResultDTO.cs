namespace dc.assignment.primenumbers.dto
{
    class EvaluateResultDTO
    {
        public string nodeName { get; set; }
        public int number { get; set; }
        public bool isPrime { get; set; }
        public int divisibleByNumber { get; set; }
    }
}