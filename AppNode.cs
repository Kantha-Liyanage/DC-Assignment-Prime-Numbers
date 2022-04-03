using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using dc.assignment.primenumbers.dto;
using dc.assignment.primenumbers.models;
using dc.assignment.primenumbers.utils.filehandler;
using dc.assignment.primenumbers.utils.serviceregister;
using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers
{
    public class AppNode
    {
        public Int64 id { get; }
        public AppNodeType type { get; set; }
        private string ipAddress;
        private int port;
        // aggregations
        private KTCPListener tcpListener;
        private PrimeNumberChecker primeNumberChecker;
        private NumbersFileHandler numbersDatFileHandler;
        private const int ELECTION_DELAY = 10000;
        public AppNode(string ipAddress, int port)
        {
            // get node id
            Random random = new Random();
            //yyyyMMdd
            this.id = Int64.Parse(DateTime.Now.ToString("HHmmssffff")) + random.Next(100, 999);

            this.ipAddress = ipAddress;
            this.port = port;

            // TCP Listener
            this.tcpListener = new KTCPListener(this.ipAddress, this.port);
            this.tcpListener.onClientRequest += processClientRequest;

            // prime number checker
            this.primeNumberChecker = new PrimeNumberChecker();
            this.primeNumberChecker.onPrimeNumberDetected += primeNumberDetected;
            this.primeNumberChecker.onPrimeNumberNotDetected += primeNumberNotDetected;

            // numbers data file
            this.numbersDatFileHandler = new NumbersFileHandler("data/numbers.txt", "data/output.txt");

            this.type = AppNodeType.Initial;

            // set node inital status
            ConsulServiceRegister.setNode(this);

            // start lifecycle method
            var worker = new Thread(() =>
            {
                process();
            });
            worker.Start();
        }

        // lifecycle method
        private void process()
        {
            int randomStartTime = new Random().Next(10000, 50000);
            Console.WriteLine("Node:" + this.id + " is frozen for " + randomStartTime);
            Thread.Sleep(randomStartTime);
            Console.WriteLine("I'm alive.");

            while (true)
            {
                // check leader
                Node node = ConsulServiceRegister.getLeader();

                // leader dead, run election
                if (node == null)
                {
                    Console.WriteLine("Leader not found! Starting an election...");

                    runElection();
                    // wait for a while and check again
                    Thread.Sleep(ELECTION_DELAY);
                }
            }
        }

        public string getAddress()
        {
            return "http://" + this.ipAddress + ":" + this.port;
        }

        public string getName()
        {
            return this.ipAddress + ":" + this.port;
        }

        private void runElection()
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
                if (node.id == this.id)
                {
                    continue;
                }

                using (var client = new HttpClient())
                {
                    VoteDTO obj = new VoteDTO();
                    obj.nodeId = this.id;
                    obj.nodeAddress = this.getAddress();

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
                // assing roles
                // Master
                this.type = AppNodeType.Master;
                ConsulServiceRegister.setLeader(this);
                ConsulServiceRegister.setNode(this);

                Console.WriteLine("I'm the leader now!!!");
            }
        }

        //===============================================================================
        // HTTP APIs of AppNode
        //===============================================================================

        // API calls routings handler
        private void processClientRequest(object? sender, KTCPListenerEventArgs e)
        {
            KHTTPResponse reponse;
            string service = e.request.resourceURL;
            HTTPMethod method = e.request.httpMethod;

            if (service.Equals("vote") && method == HTTPMethod.POST)
            {
                reponse = handleRequestVote(e.request.bodyContent);
            }
            else if (service.Equals("transform") && method == HTTPMethod.POST)
            {
                reponse = handleRequestTransform(e.request.bodyContent);
            }
            else if (service.Equals("check") && method == HTTPMethod.POST)
            {
                reponse = handleRequestCheck(e.request.bodyContent);
            }
            else if (service.Equals("abort") && method == HTTPMethod.POST)
            {
                reponse = handleRequestAbort();
            }
            else if (service.Equals("health") && method == HTTPMethod.GET)
            {
                reponse = handleRequestHealth();
            }
            else
            {
                reponse = new KHTTPResponse(HTTPResponseCode.Not_Found_404, new { message = "Resource not found" });
            }

            // send response
            reponse.sendJSON(e.tcpClient);
        }

        //===============================================================================
        // AppNode common APIs section
        //===============================================================================

        // API: vote
        private KHTTPResponse handleRequestVote(string body)
        {
            try
            {
                // Received vote request
                VoteDTO? dto = JsonSerializer.Deserialize<VoteDTO>(body);

                if (dto.nodeId > this.id)
                {
                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Younger." });
                }
                else
                {
                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Older." });
                }
            }
            catch (Exception er) { }

            return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Invalid input." });
        }

        // API: node transform
        private KHTTPResponse handleRequestTransform(string body)
        {
            throw new NotImplementedException();
        }

        // API: check
        private KHTTPResponse handleRequestCheck(string body)
        {
            // already working on somthing?
            if (this.primeNumberChecker.isChecking())
            {
                return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. A number is being chekced currently." });
            }

            try
            {
                // convert body string to object
                CheckRequestDTO? dto = JsonSerializer.Deserialize<CheckRequestDTO>(body);
                bool accepted = this.checkNumber(dto.theNumber, dto.fromNumber, dto.toNumber);
                if (accepted)
                {
                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Accepted." });
                }
            }
            catch (Exception er) { }

            return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. Invalid input." });
        }

        // API: abort
        private KHTTPResponse handleRequestAbort()
        {
            if (this.primeNumberChecker.isChecking())
            {
                this.primeNumberChecker.abort();
                return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Checking aborted." });
            }

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Already idle." });
        }

        // API: health
        private KHTTPResponse handleRequestHealth()
        {
            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "AppNode is healthy." });
        }

        //===============================================================================
        // Master section
        //===============================================================================

        //===============================================================================
        // Proposer section
        //===============================================================================
        private bool checkNumber(int theNumber, int fromNumber, int toNumber)
        {
            return this.primeNumberChecker.check(theNumber, fromNumber, toNumber);
        }

        // Inform: prime number NOT detected
        private void primeNumberNotDetected(object? sender, PrimeNumberNotDetectedEventArgs e)
        {

        }

        // Inform: prime number detected
        private void primeNumberDetected(object? sender, EventArgs e)
        {

        }

        //===============================================================================
        // Acceptor section
        //===============================================================================

        //===============================================================================
        // Learner section
        //===============================================================================
    }

    public enum AppNodeType
    {
        Master,
        Proposer,
        Acceptor,
        Learner,
        Initial // not assigned yet
    }
}