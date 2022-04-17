namespace dc.assignment.primenumbers.models
{
    public class FinalResultEventArgs
    {
        public int number { get; }
        public bool isPrime { get; }
        public int divisibleByNumber { get; }

        public FinalResultEventArgs(int number, bool isPrime, int divisibleByNumber)
        {
            this.number = number;
            this.isPrime = isPrime;
            this.divisibleByNumber = divisibleByNumber;
        }
    }
}