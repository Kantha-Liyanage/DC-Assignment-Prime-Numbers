namespace dc.assignment.primenumbers.models
{
    public class FinalResultEventArgs
    {
        public int number { get; }
        public bool isPrime { get; }

        public FinalResultEventArgs(int number, bool isPrime)
        {
            this.number = number;
            this.isPrime = isPrime;
        }
    }
}