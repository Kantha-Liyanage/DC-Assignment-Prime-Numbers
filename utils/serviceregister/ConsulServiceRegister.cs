using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using dc.assignment.primenumbers.models;

namespace dc.assignment.primenumbers.utils.serviceregister
{
    class ConsulServiceRegister
    {
        private const string SERVICE_DEREGISTER_TIME = "15s";
        private const string CHECK_INTERVAL = "5s";
        private const string CHECK_TIMEOUT = "5s";
        public static void setNode(AppNode appNode)
        {
            string[] checkArgs = { "curl", appNode.address + "/health" };

            var jsonService = new
            {
                name = "Worker:" + appNode.name,
                address = appNode.address,
                meta = new
                {
                    nodeId = appNode.id.ToString(),
                    nodeType = appNode.type.ToString()
                },
                check = new
                {
                    deregisterCriticalServiceAfter = SERVICE_DEREGISTER_TIME,
                    args = checkArgs,
                    interval = CHECK_INTERVAL,
                    timeout = CHECK_TIMEOUT
                }
            };

            var client = new HttpClient();
            // PUT and get the response.
            Task<HttpResponseMessage> response = client.PutAsJsonAsync(
                "http://localhost:8500/v1/agent/service/register",
                jsonService
            );

            var result = response.Result;
        }

        public static List<Node> getAllHealthyNodes()
        {
            AppNodeType[] nodeTypes = { };
            List<Node> nodes = ConsulServiceRegister.getHealthyNodes(
                ConsulServiceRegister.getNodes(nodeTypes)
            );
            return nodes;
        }

        public static List<Node> getAllHealthySlaveNodes()
        {
            AppNodeType[] nodeTypes = {
                AppNodeType.Proposer,
                AppNodeType.Acceptor,
                AppNodeType.Learner,
                AppNodeType.Initial
            };

            List<Node> nodes = ConsulServiceRegister.getHealthyNodes(
                ConsulServiceRegister.getNodes(nodeTypes)
            );
            return nodes;
        }


        public static List<Node> getNodes(AppNodeType[] nodeTypes)
        {
            List<Node> nodes = new List<Node>();

            string filter = "";
            foreach (AppNodeType type in nodeTypes)
            {
                if (filter.Equals(""))
                {
                    filter = " and Meta.nodeType==" + type.ToString();
                }
                else
                {
                    filter += " or Meta.nodeType==" + type.ToString();
                }
            }
            string url = "http://localhost:8500/v1/catalog/node-services/ubuntu?filter=Service!=consul" + filter;

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    // by calling .Result you are synchronously reading the result
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    // No services?
                    if (responseString.Contains("\"Services\": []"))
                    {
                        return nodes;
                    }

                    string[] part1 = responseString.Split("\"Services\": [");
                    string[] part2 = part1[1].Split("]\n}");
                    string[] part3 = part2[0].Substring(11).Split("},\n        {\n");

                    foreach (string strNode in part3)
                    {
                        Node node = new Node();
                        node.name = getValueFromJSON(strNode, "Service", false);
                        node.address = getValueFromJSON(strNode, "Address", false);
                        Int64.TryParse(getValueFromJSON(strNode, "nodeId", false), out Int64 id);
                        node.id = id;
                        Enum.TryParse(getValueFromJSON(strNode, "nodeType", true), out AppNodeType type);
                        node.type = type;

                        nodes.Add(node);
                    }
                }
            }
            return nodes;
        }

        public static List<Node> getHealthyNodes(List<Node> nodes)
        {
            List<Node> healthyNodes = new List<Node>();

            using (var client = new HttpClient())
            {
                foreach (Node node in nodes)
                {
                    var response = client.GetAsync("http://localhost:8500/v1/agent/health/service/name/" + node.name).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // by calling .Result you are synchronously reading the result
                        string responseString = response.Content.ReadAsStringAsync().Result;
                        // Status
                        bool isAlive = responseString.Contains("\"AggregatedStatus\": \"passing\"");
                        if (isAlive)
                        {
                            healthyNodes.Add(node);
                        }
                    }
                }
            }

            return healthyNodes;
        }

        public static Node getHealthyLeader()
        {
            // get all healthy nodes
            AppNodeType[] nodeTypes = { AppNodeType.Master };
            List<Node> nodes = ConsulServiceRegister.getHealthyNodes(
                ConsulServiceRegister.getNodes(nodeTypes)
            );

            if (nodes.Count == 0)
            {
                return null;
            }
            else
            {
                return nodes[0];
            }
        }

        public static List<Node> getHealthyProposers()
        {
            AppNodeType[] types = { AppNodeType.Proposer };
            List<Node> nodes = getNodes(types);
            List<Node> healthynNodes = getHealthyNodes(nodes);
            return healthynNodes;
        }

        public static List<Node> getHealthyAcceptors()
        {
            AppNodeType[] types = { AppNodeType.Acceptor };
            List<Node> nodes = getNodes(types);
            List<Node> healthynNodes = getHealthyNodes(nodes);
            return healthynNodes;
        }

        public static Node getHealthyLearner()
        {
            AppNodeType[] types = { AppNodeType.Learner };
            List<Node> nodes = getNodes(types);
            List<Node> healthynNodes = getHealthyNodes(nodes);
            return (healthynNodes.Count > 0 ? healthynNodes[0] : null);
        }

        private static string getValueFromJSON(string json, string tag, bool isLast)
        {
            string[] arr1 = json.Split("\"" + tag + "\": \"");
            string[] arr2 = isLast ? arr1[1].Split("\"\n") : arr1[1].Split("\",\n");
            return arr2[0];
        }
    }

}