using System;
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
        public AppNodeType type { get; }
        private string ipAddress;
        private int port;
        // aggregations
        KTCPListener tcpListener;
        PrimeNumberChecker primeNumberChecker;
        NumbersFileHandler numbersDatFileHandler;
        public AppNode(string ipAddress, int port)
        {
            // get node id
            Random random = new Random();
            //yyyyMMdd
            this.id = Int64.Parse(DateTime.Now.ToString("HHmmssffff")) + random.Next(1, 100);

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

            //Consul Test
            ConsulServiceRegister.setTheLeader(this.getAddress());
            //ConsulServiceRegister.clearLeader();

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
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("I'm alive.");

                // check for Master
                // no master, run an election
                Node node = ConsulServiceRegister.getLeader();
                if (node != null)
                {
                    Console.WriteLine(node.type + " is " + (node.isAlive ? "alive" : "not dead"));
                }
            }
        }

        public string getAddress()
        {
            return "http://" + this.ipAddress + ":" + this.port;
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

            if (service.Equals("transform") && method == HTTPMethod.POST)
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

        // API: node transform
        private KHTTPResponse handleRequestTransform(string bodyContent)
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