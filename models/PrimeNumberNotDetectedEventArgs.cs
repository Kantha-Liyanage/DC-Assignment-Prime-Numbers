namespace dc.assignment.primenumbers.models{
    public class PrimeNumberNotDetectedEventArgs
    {
        public int divisibleByNumber { get; }

        public PrimeNumberNotDetectedEventArgs(int divisibleByNumber){
            this.divisibleByNumber = divisibleByNumber;
        }
    }
}