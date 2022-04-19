using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using dc.assignment.primenumbers.dto;
using dc.assignment.primenumbers.utils.api;
using dc.assignment.primenumbers.utils.election;
using dc.assignment.primenumbers.utils.file;
using dc.assignment.primenumbers.utils.serviceregister;
using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers.models
{
    public class AppNode : Node
    {
        // aggregations
        private Master master;
        private Proposer proposer;
        private Acceptor acceptor;
        private Learner learner;
        private ElectionHandler electionHandler;
        private NumbersFileHelper numbersFileHelper;
        private KTCPListener tcpListener;
        private APIInvocationHandler apiInvocationHandler;
        private const int ITERATION_DELAY = 5000;

        public AppNode(string ipAddress, int port)
        {
            // get node id
            // yyyyMMdd
            this.id = Int64.Parse(DateTime.Now.ToString("HHmmssffff")) + new Random().Next(100, 999);
            this.type = AppNodeType.Initial;
            this.ipAddress = ipAddress;
            this.port = port;
            this.address = "http://" + this.ipAddress + ":" + this.port;
            this.name = this.ipAddress + ":" + this.port;

            // TCP Listener
            this.tcpListener = new KTCPListener(this.ipAddress, this.port);
            this.tcpListener.onClientRequest += processClientRequest;

            // numbers file helper
            this.numbersFileHelper = new NumbersFileHelper();

            // API handler
            this.apiInvocationHandler = new APIInvocationHandler();

            // election handler
            this.electionHandler = new ElectionHandler(this);
            this.electionHandler.onLeaderElected += electedAsTheLeader;

            // Master
            this.master = new Master(this);

            // Proposer : prime number checker
            this.proposer = new Proposer(this);
            this.proposer.onNumberEvaluationComplete += numberEvaluationComplete;

            // Acceptor
            this.acceptor = new Acceptor(this);

            // learner
            this.learner = new Learner(this);
            this.learner.onFinalResult += numberEvaluationCompleted;

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
            Program.log(this.id, this.name, "Node created and frozen for " + (randomStartTime / 1000) + "s.");

            // sleep
            Thread.Sleep(randomStartTime);

            // log
            Program.log(this.id, this.name, "Node lifecycle started.");

            while (true)
            {
                // check whether self is the leader
                if (this.type != AppNodeType.Master)
                {
                    // Learner need to check whether all Proposers are alive
                    if (this.type == AppNodeType.Learner)
                    {
                        // get Proposers
                        List<Node> proposerNodes = ConsulServiceRegister.getHealthyProposers();

                        // one or more Proposers are dead
                        if (this.learner.proposersCount != proposerNodes.Count)
                        {
                            this.reassignRoles();
                        }
                    }

                    // some other node must be the leader
                    Node node = ConsulServiceRegister.getHealthyLeader();

                    // leader dead, run election
                    if (node == null)
                    {
                        // log
                        Program.log(this.id, this.name, "Leader not found. Starting an election!");

                        // election
                        this.electionHandler.start();
                    }
                    else // leader is alive
                    {
                        // new node created after selecting the leader
                        if (this.type == AppNodeType.Initial)
                        {
                            // assign as a Proposer
                            this.type = AppNodeType.Proposer;
                            ConsulServiceRegister.setNode(this);
                        }
                    }
                }

                // leader alive, wait and see a bit
                Thread.Sleep(ITERATION_DELAY);
            }
        }

        public APIInvocationHandler getAPIInvocationHandler()
        {
            return this.apiInvocationHandler;
        }

        public NumbersFileHelper getNumbersFileHelper()
        {
            return this.numbersFileHelper;
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
            else if (service.Equals("evaluate") && method == KHTTPMethod.POST && this.type == AppNodeType.Proposer)
            {
                reponse = handleRequestEvaluate(e.request.bodyContent);
            }
            else if (service.Equals("abort") && method == KHTTPMethod.GET && this.type == AppNodeType.Proposer)
            {
                reponse = handleRequestAbort();
            }
            else if (service.Equals("setProposersCount") && method == KHTTPMethod.POST && this.type == AppNodeType.Learner)
            {
                reponse = handleRequestSetProposersCount(e.request.bodyContent);
            }
            else if (service.Equals("accept") && method == KHTTPMethod.POST && this.type == AppNodeType.Acceptor)
            {
                reponse = handleRequestAccept(e.request.bodyContent);
            }
            else if (service.Equals("learn") && method == KHTTPMethod.POST && this.type == AppNodeType.Learner)
            {
                reponse = handleRequestLearn(e.request.bodyContent);
            }
            else if (service.Equals("reset") && method == KHTTPMethod.POST && this.type == AppNodeType.Learner)
            {
                reponse = handleRequestReset();
            }
            else if (service.Equals("reassignRoles") && method == KHTTPMethod.POST && this.type == AppNodeType.Master)
            {
                reponse = handleRequestReassignRoles();
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
            // Received vote request
            VoteDTO? dto = JsonSerializer.Deserialize<VoteDTO>(body);

            if (dto.nodeId > this.id)
            {
                // log
                Program.log(this.id, this.name, "Node voted as Younger.");

                return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Younger." });
            }
            else
            {
                // log
                Program.log(this.id, this.name, "Node voted as Older.");

                return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Older." });
            }

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
                        Program.log(this.id, this.name, "Node role changed to a Proposer.");

                        break;
                    case "Acceptor":
                        this.type = AppNodeType.Acceptor;
                        ConsulServiceRegister.setNode(this);

                        // log
                        Program.log(this.id, this.name, "Node role changed to an Acceptor.");

                        break;
                    case "Learner":
                        this.type = AppNodeType.Learner;
                        ConsulServiceRegister.setNode(this);

                        // log
                        Program.log(this.id, this.name, "Node role changed to a Learner.");
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

        // Check ecosystem
        public List<Node> checkEcosystem()
        {
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
                // log
                Program.log(this.id, this.name, "Ecosystem unstable!");

                return new List<Node>(); // empty
            }

            return nodes;
        }

        public void reassignRoles()
        {
            Node master = ConsulServiceRegister.getHealthyLeader();
            if (master != null)
            {
                this.apiInvocationHandler.invokePOST(master.address + "/reassignRoles", new { });
            }
        }

        //===============================================================================
        // Master section
        //===============================================================================
        private void electedAsTheLeader(object? sender, EventArgs e)
        {
            // assign roles to all nodes
            bool success = this.master.assignRoles();
            if (!success) { return; }

            // get Proposers
            List<Node> proposerNodes = ConsulServiceRegister.getHealthyProposers();

            // inform proposers count to the Learner
            this.master.informProposersCountLearner(proposerNodes.Count);

            // blocking method: distribute the work
            this.master.distributeTasks(proposerNodes);
        }

        // API: reassign roles
        private KHTTPResponse handleRequestReassignRoles()
        {
            this.electedAsTheLeader(null, null);

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Successful." });
        }

        //===============================================================================
        // Proposer section
        //===============================================================================

        // API: check
        private KHTTPResponse handleRequestEvaluate(string body)
        {
            // already working on somthing?
            if (this.proposer.isEvaluating())
            {
                // log
                Program.log(this.id, this.name, "Not accepted. A number is being evaluated currently.");

                return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. A number is being evaluated currently." });
            }

            // convert body string to object
            EvaluateRequestDTO? dto = JsonSerializer.Deserialize<EvaluateRequestDTO>(body);
            bool accepted = this.proposer.evaluate(dto.number, dto.fromNumber, dto.toNumber);
            if (accepted)
            {
                // log
                Program.log(this.id, this.name, "Evaluation started for number: " + dto.number + " for the range from " + dto.fromNumber + " to " + dto.toNumber + ".");

                return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Accepted." });
            }

            // log
            Program.log(this.id, this.name, "Not accepted.");

            return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. Invalid input." });
        }

        // API: abort
        private KHTTPResponse handleRequestAbort()
        {
            if (this.proposer.isEvaluating())
            {
                this.proposer.abort();
                return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Evaluation aborted." });
            }

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Already idle." });
        }

        // Inform: prime number NOT detected
        private void numberEvaluationComplete(object? sender, NumberEvaluationCompleteEventArgs e)
        {
            // send result
            this.proposer.sendResultToAcceptor(this.name, e.number, e.isPrime, e.divisibleByNumber);
        }

        //===============================================================================
        // Acceptor section
        //===============================================================================
        // API: accept
        private KHTTPResponse handleRequestAccept(string body)
        {
            // convert body string to object
            EvaluateResultDTO? dto = JsonSerializer.Deserialize<EvaluateResultDTO>(body);

            // Not Prime! verify
            bool valid = this.acceptor.verify(dto.nodeName, dto.number, dto.isPrime, dto.divisibleByNumber);
            if (!valid) // Proposer is sending false results !!!
            {
                return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "False result." });
            }

            // accept and inform the Learner
            this.acceptor.accept(dto.number, dto.isPrime, dto.divisibleByNumber);

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Accepted." });
        }

        //===============================================================================
        // Learner section
        //===============================================================================
        // API: set Proposers count
        private KHTTPResponse handleRequestSetProposersCount(string body)
        {
            // convert body string to object
            ProposersCountDTO? dto = JsonSerializer.Deserialize<ProposersCountDTO>(body);

            // set count
            this.learner.proposersCount = dto.proposers;

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Accepted." });
        }

        // API: learn
        private KHTTPResponse handleRequestLearn(string body)
        {
            // convert body string to object
            PrimeResultDTO? dto = JsonSerializer.Deserialize<PrimeResultDTO>(body);

            // learn
            this.learner.learn(dto.number, dto.isPrime, dto.divisibleByNumber);

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Accepted." });
        }

        // API: reset
        private KHTTPResponse handleRequestReset()
        {
            // reset
            this.learner.reset();

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Successful." });
        }

        // Current number evaluation completed   
        private void numberEvaluationCompleted(object? sender, FinalResultEventArgs e)
        {
            // write the result to the output file 
            // and release the current number
            // so that master node can take the next number
            this.learner.completeNumber(e.number, e.isPrime, e.divisibleByNumber);
        }
    }
}