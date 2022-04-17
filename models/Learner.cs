using System;
using System.Collections.Generic;

namespace dc.assignment.primenumbers.models
{
    public class Learner
    {
        public int proposersCount { get; set; }
        public event EventHandler<FinalResultEventArgs>? onFinalResult;
        private int learnCount = 0;
        private List<Result> results = new List<Result>();
        private AppNode appNode;

        public Learner(AppNode appNode)
        {
            this.appNode = appNode;
        }
        public void learn(int number, bool isPrime)
        {
            results.Add(new Result(number, isPrime));
            learnCount++;

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