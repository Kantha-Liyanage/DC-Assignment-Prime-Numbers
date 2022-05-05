using System;
using System.Collections.Generic;
using System.Threading;
using dc.assignment.primenumbers.utils.serviceregister;

namespace dc.assignment.primenumbers.models
{
    public class Master
    {
        private AppNode appNode;
        private int NEXT_NUMBER_GET_DELAY = 5000;

        public Master(AppNode appNode)
        {
            this.appNode = appNode;
        }

        public bool assignRoles()
        {
            // Master
            this.appNode.type = AppNodeType.Master;
            ConsulServiceRegister.setNode(this.appNode);
            Console.Title = "NodeType : " + this.appNode.type;

            // check the consistancy of the ecosystem
            List<Node> nodes = checkEcosystem();

            // check ecosystem
            if (nodes.Count == 0)
            {
                // log
                Program.log(this.appNode.id, this.appNode.name, "No nodes found.");

                // revert
                this.appNode.type = AppNodeType.Initial;
                ConsulServiceRegister.setNode(this.appNode);
                Console.Title = "NodeType : " + this.appNode.type;
                return false;
            }
            else
            {
                // log
                Program.log(this.appNode.id, this.appNode.name, "Node is the leader now!");

                // other roles
                // Step 1: abort currently running jobs
                foreach (Node node in nodes)
                {
                    if (node.type == AppNodeType.Proposer)
                    {
                        // abort 
                        string responseStr = this.appNode.getAPIInvocationHandler().invokeGET(node.address + "/abort");
                    }
                    else if (node.type == AppNodeType.Learner)
                    {
                        // reset 
                        string responseStr = this.appNode.getAPIInvocationHandler().invokeGET(node.address + "/reset");
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
                Program.log(this.appNode.id, this.appNode.name, "New roles assigned.");
                return true;
            }
        }

        // Check ecosystem
        private List<Node> checkEcosystem()
        {
            List<Node> nodes = ConsulServiceRegister.getAllHealthySlaveNodes();

            // check ecosystem
            if (nodes.Count < 5)
            {
                // log
                Program.log(this.appNode.id, this.appNode.name, "Ecosystem unstable!");

                return new List<Node>(); // empty
            }

            return nodes;
        }

        public void informProposersCountLearner(int proposerNodesCount)
        {
            // get the Learner
            Node learner = ConsulServiceRegister.getHealthyLearner();

            // update number of proposers in the ecosystem
            string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(learner.address + "/setProposersCount", new { proposers = proposerNodesCount });

            // Learner is dead
            if (responseStr == null)
            {
                // check ecosystem and reassign roles
                this.appNode.reassignRoles();
                // abort
                return;
            }

            // log
            Program.log(this.appNode.id, this.appNode.name, "Learner : " + learner.name + " must receive results from " + proposerNodesCount + " proposers.");
        }

        public void distributeTasks(List<Node> nodes)
        {
            // until all numbers are evaluated
            int previousNumber = -1;
            int nextNumber = 0;
            while (true)
            {
                // get next number
                nextNumber = this.appNode.getNumbersFileHelper().getNextNumber();

                // eof or no number in the file
                if (nextNumber == -1)
                {
                    // log
                    Program.log(this.appNode.id, this.appNode.name, "End of numbers file!");
                    return;
                }
                // previous number still not completed
                else if (previousNumber == nextNumber)
                {
                    // sleep
                    Thread.Sleep(NEXT_NUMBER_GET_DELAY);
                    continue;
                }

                // log
                Program.log(this.appNode.id, this.appNode.name, "Next number " + nextNumber + " released.");

                // number range distribution
                int fullPortionCount = nextNumber / nodes.Count;
                int remainder = nextNumber % nodes.Count;
                int nodeIndex = 0;
                int previousNodeToNumber = 2;
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
                    Program.log(this.appNode.id, this.appNode.name, "Node: " + node.name + " was assigned to evaluate the range " + node.fromNumber + " - " + node.toNumber + " of number " + nextNumber + ".");

                    // assign task
                    var evaluateRequest = new
                    {
                        number = nextNumber,
                        fromNumber = node.fromNumber,
                        toNumber = node.toNumber
                    };
                    string responseStr = this.appNode.getAPIInvocationHandler().invokePOST(node.address + "/evaluate", evaluateRequest);

                    // Proposer is not reachable
                    if (responseStr == null)
                    {
                        // check ecosystem and reassign roles
                        this.appNode.reassignRoles();
                        // abort 
                        return;
                    }
                }

                previousNumber = nextNumber;
            }
        }
    }
}