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
            // Prime
            if (divisibleByNumber == 0)
            {
                // log
                Program.log(this.appNode.id, this.appNode.name, "Verification completed. 🆗");

                return true;
            }

            int remainder = number % divisibleByNumber;
            // Not Prime! verify
            if (!isPrime && remainder != 0) // Proposer is sending false results !!!
            {
                // log
                Program.log(this.appNode.id, this.appNode.name, "Proposer: " + nodeName + " is sending false results. 🚨");
                return false;
            }

            // log
            Program.log(this.appNode.id, this.appNode.name, "Verification completed. 🆗");

            return true;
        }

        public void accept(int number, bool isPrime, int divisibleByNumber)
        {
            // log
            Program.log(this.appNode.id, this.appNode.name, "Range evaluation result accepted. 👍");

            // inform the Learner
            Node learnerNode = ConsulServiceRegister.getHealthyLearner();

            // there must be a Learner node
            if (learnerNode == null)
            {
                // check ecosystem and reassign roles
                this.appNode.reassignRoles();
                return;
            }

            var obj = new
            {
                number = number,
                isPrime = isPrime,
                divisibleByNumber = divisibleByNumber
            };

            // log
            Program.log(this.appNode.id, this.appNode.name, "Informing the Learner...👨‍🏫");

            // need to check ecosystem
            string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(learnerNode.address + "/learn", obj);

            // Learner is not reachable
            if (responseStr == null)
            {
                // check ecosystem and reassign roles
                this.appNode.reassignRoles();
            }
        }

    }
}