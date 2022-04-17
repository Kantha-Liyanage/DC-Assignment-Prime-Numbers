using System;
using System.Collections.Generic;

namespace dc.assignment.primenumbers.models
{
    public class Learner
    {
        private int _proposersCount;
        public event EventHandler<FinalResultEventArgs>? onFinalResult;
        private int learnCount = 0;
        private List<Result> results = new List<Result>();
        private AppNode appNode;

        public Learner(AppNode appNode)
        {
            this.appNode = appNode;
        }

        public int proposersCount
        {
            get { return _proposersCount; }
            set
            {
                _proposersCount = value;

                // log
                Program.log(this.appNode.id, this.appNode.name, "There are " + value + " proposers.");
            }
        }
        public void learn(int number, bool isPrime)
        {
            results.Add(new Result(number, isPrime));
            learnCount++;

            // log
            Program.log(this.appNode.id, this.appNode.name, "A result received for number: " + number + " as " + (isPrime ? "Prime." : "Not Prime."));

            if (learnCount == proposersCount)
            {
                // final result
                bool isPrimeFinal = true;
                foreach (Result result in results)
                {
                    if (!result.isPrime) // Not prime!!!
                    {
                        isPrimeFinal = false;
                    }
                }

                // log
                Program.log(this.appNode.id, this.appNode.name, "All proposers responded. Number: " + number + (isPrime ? " is Prime." : " is not Prime."));

                // fire the event
                onFinalResult?.Invoke(this, new FinalResultEventArgs(number, isPrimeFinal));

                // reset to accept results of the next number
                results.Clear();
                learnCount = 0;
            }
        }

        public void completeNumber(int number, bool isPrime)
        {
            this.appNode.getNumbersFileHelper().completeNumber(number, isPrime);

            // log
            Program.log(this.appNode.id, this.appNode.name, "Number: " + number + " completed.");
        }
    }

    class Result
    {
        public int number { get; set; }
        public bool isPrime { get; set; }

        public Result(int number, bool isPrime)
        {
            this.number = number;
            this.isPrime = isPrime;
        }
    }
}