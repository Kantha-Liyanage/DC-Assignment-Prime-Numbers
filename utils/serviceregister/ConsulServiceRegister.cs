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
        private const string CHECK_INTERVAL = "3s";
        private const string CHECK_TIMEOUT = "2s";

        public static bool setLeader(AppNode appNode)
        {
            string[] checkArgs = { "curl", appNode.getAddress() + "/health" };

            var jsonService = new
            {
                name = "leader",
                address = appNode.getAddress(),
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

            var response = client.PutAsJsonAsync(
                 "http://localhost:8500/v1/agent/service/register",
                 jsonService
             ).Result;

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Node getLeader()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://localhost:8500/v1/agent/health/service/name/leader").Result;
                if (response.IsSuccessStatusCode)
                {
                    // by calling .Result you are synchronously reading the result
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    // Node
                    Node node = new Node();
                    node.name = "master";
                    node.type = AppNodeType.Master;
                    node.address = getValueFromJSON(responseString, "Address", false);
                    // Status
                    node.isAlive = responseString.Contains("\"AggregatedStatus\": \"passing\"");
                    return node;
                }
            }

            return null;
        }

        public static Task<HttpResponseMessage> clearLeader()
        {
            var client = new HttpClient();
            // PUT and get the response.
            Task<HttpResponseMessage> response = client.PutAsJsonAsync(
                "http://localhost:8500/v1/agent/service/deregister/leader",
                new { }
            );

            return response;
        }

        public static Task<HttpResponseMessage> setNode(AppNode appNode)
        {
            string[] checkArgs = { "curl", appNode.getAddress() + "/health" };

            var jsonService = new
            {
                name = "node:" + appNode.getName(),
                address = appNode.getAddress(),
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

            return response;
        }

        public static List<Node> getNodes(AppNodeType[] nodeTypes)
        {
            List<Node> nodes = new List<Node>();

            string filter = "";
            foreach (AppNodeType type in nodeTypes)
            {
                if (filter.Equals(""))
                {
                    filter = "and Meta.nodeType==" + type.ToString();
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
                        node.isAlive = responseString.Contains("\"AggregatedStatus\": \"passing\"");
                        if (node.isAlive)
                        {
                            healthyNodes.Add(node);
                        }
                    }
                }
            }

            return healthyNodes;
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

        public static List<Node> getHealthyLearner()
        {
            AppNodeType[] types = { AppNodeType.Learner };
            List<Node> nodes = getNodes(types);
            List<Node> healthynNodes = getHealthyNodes(nodes);
            return healthynNodes;
        }

        private static string getValueFromJSON(string json, string tag, bool isLast)
        {
            string[] arr1 = json.Split("\"" + tag + "\": \"");
            string[] arr2 = isLast ? arr1[1].Split("\"\n") : arr1[1].Split("\",\n");
            return arr2[0];
        }
    }

}