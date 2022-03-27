using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using dc.assignment.primenumbers.models;

namespace dc.assignment.primenumbers.utils.serviceregister
{
    class ConsulServiceRegister
    {
        public static Task<HttpResponseMessage> setLeader(string appNodeAddress)
        {
            string[] checkArgs = { "curl", appNodeAddress + "/health" };

            var jsonService = new
            {
                name = "leader",
                address = appNodeAddress,
                check = new
                {
                    deregisterCriticalServiceAfter = "90m",
                    args = checkArgs,
                    interval = "5s",
                    timeout = "5s"
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

        public static Task<HttpResponseMessage> setTheLeader(string appNodeAddress)
        {
            Node node = new Node();
            node.name = "leader";
            node.address = appNodeAddress;
            node.meta.nodeType = AppNodeType.Master;
            node.check.arg = new string[] { "curl", appNodeAddress + "/health" };

            var client = new HttpClient();
            // PUT and get the response.
            Task<HttpResponseMessage> response = client.PutAsJsonAsync(
                "http://localhost:8500/v1/agent/service/register",
                node
            );

            return response;
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

                    // node check
                    bool isAlive = responseString.Contains("\"AggregatedStatus\": \"passing\"");

                    if (isAlive)
                    {
                        // Node
                        Node node = new Node();
                        node.name = "master";
                        node.meta.nodeType = AppNodeType.Master;
                        //Address
                        string[] arr1 = responseString.Split("\"Address\": \"");
                        string[] arr2 = arr1[1].Split("\",\n");
                        node.address = arr2[0];
                        return node;
                    }
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

        public void setNode() { }
        public void getAllNodes() { }

        public void getProposers() { }

        public void getAcceptors() { }

        public void getLearner() { }

    }

}