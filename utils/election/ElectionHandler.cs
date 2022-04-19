using System;
using System.Collections.Generic;
using System.Text.Json;
using dc.assignment.primenumbers.dto;
using dc.assignment.primenumbers.models;
using dc.assignment.primenumbers.utils.serviceregister;

namespace dc.assignment.primenumbers.utils.election
{
    public class ElectionHandler
    {
        public event EventHandler? onLeaderElected;
        private AppNode appNode;

        public ElectionHandler(AppNode appNode)
        {
            this.appNode = appNode;
        }

        public void start()
        {
            // get all healthy nodes
            AppNodeType[] nodeTypes = { };
            List<Node> nodes = ConsulServiceRegister.getHealthyNodes(
                ConsulServiceRegister.getNodes(nodeTypes)
            );

            // Reqeust vote from each node
            int olderCount = 0;
            foreach (Node node in nodes)
            {
                // avoid self
                if (node.id == appNode.id)
                {
                    continue;
                }

                var obj = new
                {
                    nodeId = appNode.id,
                    nodeAddress = appNode.address
                };
                string responseString = this.appNode.getAPIInvocationHandler().invokePOST(node.address + "/vote", obj);

                // node is dead
                if (responseString == null)
                {
                    // abort
                    return;
                }

                if (responseString.Contains("Younger"))
                {
                    olderCount++;
                }
            }

            // All nodes have confirmed that this node is the Oldest 
            if (olderCount == (nodes.Count - 1))
            {
                onLeaderElected?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}