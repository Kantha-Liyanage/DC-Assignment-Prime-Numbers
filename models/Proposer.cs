using System;
using System.Collections.Generic;
using System.Threading;
using dc.assignment.primenumbers.utils.serviceregister;

namespace dc.assignment.primenumbers.models
{
    class Proposer
    {
        private bool _isEvaluating;
        private bool _abort;
        private AppNode appNode;
        public event EventHandler<NumberEvaluationCompleteEventArgs>? onNumberEvaluationComplete;
        private const int DEMO_DELAY = 1;
        public Proposer(AppNode appNode)
        {
            this.appNode = appNode;
            this._isEvaluating = false;
            this._abort = false;
        }

        public bool isEvaluating()
        {
            return _isEvaluating;
        }

        public void abort()
        {
            this._isEvaluating = false;
            this._abort = true;
        }

        public bool evaluate(int theNumber, int fromNumber, int toNumber)
        {
            if (!isValidInput(theNumber, fromNumber, toNumber))
            {
                // log
                Program.log(this.appNode.id, this.appNode.name, "Invalid input.");

                return false;
            }

            this._isEvaluating = true;
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

                    // sleep
                    Thread.Sleep(DEMO_DELAY);
                }

                // inform 
                if (!this._abort)
                {
                    // log
                    Program.log(this.appNode.id, this.appNode.name, "Range evaluation completed. The number " + (isPrimeNumber ? "could be Prime." : "is not Prime. Divisible by: " + currentNumber + "."));

                    if (isPrimeNumber)
                    {
                        onNumberEvaluationComplete?.Invoke(this, new NumberEvaluationCompleteEventArgs(theNumber, isPrimeNumber, 0));
                    }
                    else
                    {
                        // can be devide by "currentNumber"
                        onNumberEvaluationComplete?.Invoke(this, new NumberEvaluationCompleteEventArgs(theNumber, isPrimeNumber, currentNumber));
                    }
                }
                this._isEvaluating = false;
            });
            thread.Start();

            return true;
        }

        public void sendResultToAcceptor(string nodeName, int number, bool isPrime, int divisibleByNumber)
        {
            // get Acceptors
            List<Node> acceptorNodes = ConsulServiceRegister.getHealthyAcceptors();

            // check ecosystem
            // There must be at least minimum of 2 Acceptors
            if (acceptorNodes.Count < 2)
            {
                this.appNode.master.assignRoles();
                return;
            }

            // pick one random Acceptor
            int acceptorRandomIndex = new Random().Next(0, acceptorNodes.Count);

            // log
            Program.log(this.appNode.id, this.appNode.name, "Acceptor random index: " + acceptorRandomIndex + " out of " + acceptorNodes.Count + ".");

            Node acceptorNode = acceptorNodes[acceptorRandomIndex];

            // log
            Program.log(this.appNode.id, this.appNode.name, "Acceptor: " + acceptorNode.name + " was selected.");

            // send result
            var obj = new
            {
                nodeName = nodeName,
                number = number,
                isPrime = isPrime,
                divisibleByNumber = divisibleByNumber
            };

            // log
            Program.log(this.appNode.id, this.appNode.name, "Informing to the selected Acceptor...");

            string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(acceptorNode.address + "/accept", obj);
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