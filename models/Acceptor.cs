using System;
using dc.assignment.primenumbers.utils.serviceregister;

namespace dc.assignment.primenumbers.models
{
    class Acceptor
    {
        private AppNode appNode;
        public Acceptor(AppNode appNode)
        {
            this.appNode = appNode;
        }

        public bool verify(string nodeName, int number, bool isPrime, int divisibleByNumber)
        {
            int remainder = number % divisibleByNumber;
            // Not Prime! verify
            if (!isPrime && remainder != 0) // Proposer is sending false results !!!
            {
                // log
                Program.log(this.appNode.id, this.appNode.name, "Proposer: " + nodeName + " is sending false results");
                return false;
            }

            return true;
        }

        public void accept(int number, bool isPrime)
        {
            // inform the Learner
            Node learnerNode = ConsulServiceRegister.getHealthyLearner();
            var obj = new
            {
                number = number,
                isPrime = isPrime
            };

            // need to check ecosystem
            string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(learnerNode.address + "/learn", obj);
        }

    }
}