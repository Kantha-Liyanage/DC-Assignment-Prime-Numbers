using System.Collections.Generic;
using dc.assignment.primenumbers.utils.serviceregister;

namespace dc.assignment.primenumbers.models
{
    public class Master
    {
        private AppNode appNode;

        public Master(AppNode appNode)
        {
            this.appNode = appNode;
        }

        public bool assignRoles()
        {
            // Master
            this.appNode.type = AppNodeType.Master;
            ConsulServiceRegister.setNode(this.appNode);

            // check the consistancy of the ecosystem
            List<Node> nodes = this.appNode.checkEcosystem();

            // check ecosystem
            if (nodes.Count == 0)
            {
                // revert
                this.appNode.type = AppNodeType.Initial;
                ConsulServiceRegister.setNode(this.appNode);
                return false;
            }
            else
            {
                // log
                Program.logger.log(this.appNode.id, this.appNode.name, "Node is the leader now. 🤴");

                // other roles
                // Step 1: abort currently running Proposer jobs
                foreach (Node node in nodes)
                {
                    if (node.type == AppNodeType.Proposer)
                    {
                        // abort 
                        string responseStr = this.appNode.getAPIInvocationHandler().invokeGET(node.address + "/abort");
                    }
                }

                // Step 2: assign new roles
                int nodeIndex = 0;
                foreach (Node node in nodes)
                {
                    nodeIndex++;

                    // two Acceptors
                    if (nodeIndex <= 2)
                    {
                        string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(node.address + "/transform", new { role = "Acceptor" });
                    }

                    // one Learner
                    else if (nodeIndex == 3)
                    {
                        string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(node.address + "/transform", new { role = "Learner" });
                    }

                    // rest are Proposers
                    else
                    {
                        string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(node.address + "/transform", new { role = "Proposer" });
                    }
                }

                // log
                Program.logger.log(this.appNode.id, this.appNode.name, "New roles assigned. 🪄");
                return true;
            }
        }

        public void informProposersCountLearner(int proposerNodesCount)
        {
            // get the Learner
            Node learner = ConsulServiceRegister.getHealthyLearner();

            // update number of proposers in the ecosystem
            string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(learner.address + "/setProposersCount", new { proposers = proposerNodesCount });
        }

        public void distributeTasks(List<Node> nodes)
        {
            // until all numbers are evaluated
            while (true)
            {
                // get next number
                int nextNumber = this.appNode.getNumbersFileHelper().getNextNumber();

                // eof or no number in the file
                if (nextNumber == -1)
                {
                    return;
                }
                // previous number still not completed
                else if (nextNumber == 0)
                {
                    continue;
                }

                // log
                Program.logger.log(this.appNode.id, this.appNode.name, "Next number " + nextNumber + " released.");

                // number range distribution
                int fullPortionCount = nextNumber / nodes.Count;
                int remainder = nextNumber % nodes.Count;
                int nodeIndex = 0;
                int previousNodeToNumber = 1;
                foreach (Node node in nodes)
                {
                    nodeIndex++;
                    node.fromNumber = previousNodeToNumber;
                    node.toNumber = nodeIndex * fullPortionCount;
                    previousNodeToNumber = node.toNumber + 1;

                    // add odd number to the last node
                    if (nodes.Count == nodeIndex)
                    {
                        node.toNumber += remainder;
                    }

                    // log
                    Program.logger.log(this.appNode.id, this.appNode.name, "Node: " + node.name + " was assigned to evaluate the range " + node.fromNumber + " - " + node.toNumber + " of number " + nextNumber + ". 🔢");

                    // assign task
                    var evaluateRequest = new
                    {
                        number = nextNumber,
                        fromNumber = node.fromNumber,
                        toNumber = node.toNumber
                    };

                    // need to check ecosystem
                    string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(node.address + "/evaluate", evaluateRequest);
                }
            }
        }
    }
}