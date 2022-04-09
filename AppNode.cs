using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using dc.assignment.primenumbers.dto;
using dc.assignment.primenumbers.models;
using dc.assignment.primenumbers.utils.api;
using dc.assignment.primenumbers.utils.election;
using dc.assignment.primenumbers.utils.file;
using dc.assignment.primenumbers.utils.serviceregister;
using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers
{
    public class AppNode : Node
    {
        // aggregations
        private KTCPListener tcpListener;
        private PrimeNumberChecker primeNumberChecker;
        private NumbersFileHandler numbersDatFileHandler;
        private ElectionHandler electionHandler;
        private APIInvocationHandler apiInvocationHandler;
        private const int ELECTION_DELAY = 10000;
        private const int LEADER_DETECTION_DELAY = 3000;
        public AppNode(string ipAddress, int port)
        {
            // get node id
            Random random = new Random();
            //yyyyMMdd
            this.id = Int64.Parse(DateTime.Now.ToString("HHmmssffff")) + random.Next(100, 999);
            this.type = AppNodeType.Initial;
            this.ipAddress = ipAddress;
            this.port = port;
            this.address = "http://" + this.ipAddress + ":" + this.port;
            this.name = this.ipAddress + ":" + this.port;

            // TCP Listener
            this.tcpListener = new KTCPListener(this.ipAddress, this.port);
            this.tcpListener.onClientRequest += processClientRequest;

            // API handler
            this.apiInvocationHandler = new APIInvocationHandler();

            // prime number checker
            this.primeNumberChecker = new PrimeNumberChecker();
            this.primeNumberChecker.onPrimeNumberDetected += primeNumberDetected;
            this.primeNumberChecker.onPrimeNumberNotDetected += primeNumberNotDetected;

            // numbers data file
            this.numbersDatFileHandler = new NumbersFileHandler("data/numbers.txt", "data/output.txt");

            // election handler
            electionHandler = new ElectionHandler(this);
            electionHandler.onLeaderElected += electedAsTheLeader;

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
            // sleeping time
            int randomStartTime = new Random().Next(10000, 20000);
            // log
            Program.logger.log(this.id, this.name, "Node created and frozen for " + (randomStartTime / 1000) + "s ⏳");

            // sleep
            Thread.Sleep(randomStartTime);

            // log
            Program.logger.log(this.id, this.name, "Node lifecycle started.🟢");

            while (true)
            {
                // check leader
                Node node = ConsulServiceRegister.getHealthyLeader();

                // leader dead, run election
                if (node == null)
                {
                    // log
                    Program.logger.log(this.id, this.name, "Leader not found. Starting an election...📢");

                    // election
                    electionHandler.start();

                    // wait for a while and check again
                    Thread.Sleep(ELECTION_DELAY);
                }
                else
                {
                    // leader detection delay
                    Thread.Sleep(LEADER_DETECTION_DELAY);
                }
            }
        }

        public APIInvocationHandler getAPIInvocationHandler()
        {
            return this.apiInvocationHandler;
        }

        //===============================================================================
        // HTTP APIs of AppNode
        //===============================================================================

        // API calls routings handler
        private void processClientRequest(object? sender, KTCPListenerEventArgs e)
        {
            KHTTPResponse reponse;
            string service = e.request.resourceURL;
            KHTTPMethod method = e.request.httpMethod;

            if (service.Equals("health") && method == KHTTPMethod.GET)
            {
                reponse = handleRequestHealth();
            }
            else if (service.Equals("vote") && method == KHTTPMethod.POST)
            {
                reponse = handleRequestVote(e.request.bodyContent);
            }
            else if (service.Equals("transform") && method == KHTTPMethod.POST)
            {
                reponse = handleRequestTransform(e.request.bodyContent);
            }
            else if (service.Equals("check") && method == KHTTPMethod.POST && this.type == AppNodeType.Proposer)
            {
                reponse = handleRequestCheck(e.request.bodyContent);
            }
            else if (service.Equals("abort") && method == KHTTPMethod.GET && this.type == AppNodeType.Proposer)
            {
                reponse = handleRequestAbort();
            }
            else
            {
                reponse = new KHTTPResponse(HTTPResponseCode.Not_Found_404, new { message = "Resource not found." });
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
                    // log
                    Program.logger.log(this.id, this.name, "Node voted as Younger. ✅");

                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Younger." });
                }
                else
                {
                    // log
                    Program.logger.log(this.id, this.name, "Node voted as Older. ⭕");

                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Older." });
                }
            }
            catch (Exception er) { }

            return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Invalid input." });
        }

        // API: node transform
        private KHTTPResponse handleRequestTransform(string body)
        {
            try
            {
                // Received transform request
                RoleDTO? dto = JsonSerializer.Deserialize<RoleDTO>(body);
                switch (dto.role)
                {
                    case "Proposer":
                        this.type = AppNodeType.Proposer;
                        ConsulServiceRegister.setNode(this);

                        // log
                        Program.logger.log(this.id, this.name, "Node role changed to a Proposer. 🧮");

                        break;
                    case "Acceptor":
                        this.type = AppNodeType.Acceptor;
                        ConsulServiceRegister.setNode(this);

                        // log
                        Program.logger.log(this.id, this.name, "Node role changed to an Acceptor. 📥");

                        break;
                    case "Learner":
                        this.type = AppNodeType.Learner;
                        ConsulServiceRegister.setNode(this);

                        // log
                        Program.logger.log(this.id, this.name, "Node role changed to a Learner. 💡");
                        break;
                    default:
                        // error
                        return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Invalid input." });
                }

                // success
                return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Node role changed." });
            }
            catch (Exception er) { }

            // error
            return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Invalid input." });
        }

        // API: health
        private KHTTPResponse handleRequestHealth()
        {
            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "AppNode is healthy." });
        }

        //===============================================================================
        // Master section
        //===============================================================================
        private void electedAsTheLeader(object? sender, EventArgs e)
        {
            // Master
            this.type = AppNodeType.Master;
            ConsulServiceRegister.setNode(this);

            // get all healthy nodes and assign roles
            AppNodeType[] nodeTypes = {
                AppNodeType.Proposer,
                AppNodeType.Acceptor,
                AppNodeType.Learner,
                AppNodeType.Initial
            };
            List<Node> nodes = ConsulServiceRegister.getHealthyNodes(
                ConsulServiceRegister.getNodes(nodeTypes)
            );

            // check ecosystem
            if (nodes.Count < 5)
            {
                // revert
                this.type = AppNodeType.Initial;
                ConsulServiceRegister.setNode(this);

                // log
                Program.logger.log(this.id, this.name, "Ecosystem unstable. 🚑");
            }
            else
            {
                // log
                Program.logger.log(this.id, this.name, "Node is the leader now. 🤴");

                // other roles
                // Step 1: abort currently running Proposer jobs
                foreach (Node node in nodes)
                {
                    if (node.type == AppNodeType.Proposer)
                    {
                        // abort 
                        this.apiInvocationHandler.invokeGET(node.address + "/abort");
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
                        this.apiInvocationHandler.invokePOST(node.address + "/transform", "{\"role\":\"Acceptor\"}");
                    }

                    // one Learner
                    else if (nodeIndex == 3)
                    {
                        this.apiInvocationHandler.invokePOST(node.address + "/transform", "{\"role\":\"Learner\"}");
                    }

                    // rest are Proposers
                    else
                    {
                        this.apiInvocationHandler.invokePOST(node.address + "/transform", "{\"role\":\"Proposer\"}");
                    }
                }

                // log
                Program.logger.log(this.id, this.name, "New roles assigned. 🪄");
            }
        }

        //===============================================================================
        // Proposer section
        //===============================================================================

        // API: check
        private KHTTPResponse handleRequestCheck(string body)
        {
            // already working on somthing?
            if (this.primeNumberChecker.isChecking())
            {
                return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. A number is being checked currently." });
            }

            try
            {
                // convert body string to object
                CheckRequestDTO? dto = JsonSerializer.Deserialize<CheckRequestDTO>(body);
                bool accepted = this.primeNumberChecker.check(dto.theNumber, dto.fromNumber, dto.toNumber);
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
}