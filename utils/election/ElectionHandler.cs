using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

                using (var client = new HttpClient())
                {
                    VoteDTO obj = new VoteDTO();
                    obj.nodeId = appNode.id;
                    obj.nodeAddress = appNode.address;

                    try
                    {
                        var content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
                        var response = client.PostAsync(node.address + "/vote", content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = response.Content.ReadAsStringAsync().Result;
                            if (responseString.Contains("Older"))
                            {
                                olderCount++;
                            }
                        }
                    }
                    catch (Exception er)
                    {
                        Console.WriteLine("Error: " + er.Message);
                    }
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