using System;
using System.Threading;

namespace dc.assignment.primenumbers.models
{
    class PrimeNumberChecker
    {
        private bool _isChecking;
        private bool _abort;
        public event EventHandler? onPrimeNumberDetected;
        public event EventHandler<PrimeNumberNotDetectedEventArgs>? onPrimeNumberNotDetected;
        public PrimeNumberChecker()
        {
            this._isChecking = false;
            this._abort = false;
        }

        public bool isChecking()
        {
            return _isChecking;
        }

        public void abort()
        {
            this._isChecking = false;
            this._abort = true;
        }

        public bool check(int theNumber, int fromNumber, int toNumber)
        {
            if (!isValidInput(theNumber, fromNumber, toNumber))
            {
                return false;
            }

            this._isChecking = true;
            this._abort = false;

            var thread = new Thread(() =>
            {
                // work of the work
                int currentNumber = fromNumber;
                bool isPrimeNumber = true;

                while (currentNumber <= toNumber)
                {
                    // abort check
                    if (this._abort)
                    {
                        break;
                    }

                    //Thread.Sleep(1);
                    if (theNumber % currentNumber == 0)
                    {
                        if (theNumber == currentNumber)
                        {
                            isPrimeNumber = true;
                            break;
                        }
                        else
                        {
                            isPrimeNumber = false;
                            break;
                        }
                    }
                    currentNumber++;
                }

                // inform 
                if (!this._abort)
                {
                    if (isPrimeNumber)
                    {
                        onPrimeNumberDetected?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        // can be devide by "currentNumber"
                        onPrimeNumberNotDetected?.Invoke(this, new PrimeNumberNotDetectedEventArgs(currentNumber));
                    }
                }
                this._isChecking = false;
            });
            thread.Start();

            return true;
        }

        private bool isValidInput(int theNumber, int fromNumber, int toNumber)
        {
            if (theNumber <= 2 || fromNumber <= 1 || toNumber <= 1)
            {
                return false;
            }

            if (toNumber < fromNumber)
            {
                return false;
            }

            return true;
        }
    }
}