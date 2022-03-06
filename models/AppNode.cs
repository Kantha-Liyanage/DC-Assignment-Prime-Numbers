using System.Text.Json;
using dc.assignment.primenumbers.dto;
using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers.models{
    public class AppNode {
        public Int64 id {get;}
        private string ipAddress;
        private int port;
        // aggregations
        KTCPListener tcpListener;
        PrimeNumberChecker primeNumberChecker;
        public AppNode(string ipAddress, int port){
            // get node id
            Random random = new Random();
            this.id = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff")) + random.Next(1, 100);

            this.ipAddress = ipAddress;
            this.port = port;

            // TCP Listener
            this.tcpListener = new KTCPListener(this.ipAddress,this.port);
            this.tcpListener.onClientRequest += processClientRequest;

            // prime number checker
            this.primeNumberChecker = new PrimeNumberChecker();
            this.primeNumberChecker.onPrimeNumberDetected += primeNumberDetected;
            this.primeNumberChecker.onPrimeNumberNotDetected += primeNumberNotDetected;
        }

        public string getAddress(){
            return this.ipAddress + ":" + this.port;
        }

        // API calls routings handler
        private void processClientRequest(object? sender, KTCPListenerEventArgs e)
        {
            KHTTPResponse reponse;

            if(e.request.resourceURL.Equals("check") && e.request.httpMethod == HTTPMethod.POST){
                reponse = handleRequestCheck(e.request.bodyContent);
            }
            else if(e.request.resourceURL.Equals("abort") && e.request.httpMethod == HTTPMethod.POST){
                reponse = handleRequestAbort();
            }
            else if(e.request.resourceURL.Equals("health") && e.request.httpMethod == HTTPMethod.GET){
                reponse = handleRequestHealth();
            }
            else{
                reponse = new KHTTPResponse(HTTPResponseCode.Not_Found_404, new { message = "Resource not found" });
            }

            // send response
            reponse.send(e.tcpClient);
        }

        // API: check
        private KHTTPResponse handleRequestCheck(string body)
        {
            // already working on somthing?
            if(this.primeNumberChecker.isChecking()){
                return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. A number is being chekced currently." });
            }

            // convert body string to object
            try{
                CheckRequestDTO? dto = JsonSerializer.Deserialize<CheckRequestDTO>(body);
                bool accepted = this.primeNumberChecker.check(dto.theNumber,dto.fromNumber,dto.toNumber);
                if(accepted){
                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Accepted." });
                }
            }
            catch(Exception er){
                
            }

            return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. Invalid input." });
        }

        // API: abort
        private KHTTPResponse handleRequestAbort()
        {
            if(this.primeNumberChecker.isChecking()){
                this.primeNumberChecker.abort();
                return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Checking aborted." });
            }

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Already idle."});
        }

        // API: health
        private KHTTPResponse handleRequestHealth(){
            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "AppNode is healthy."});
        }

        // Inform: prime number NOT detected
        private void primeNumberNotDetected(object? sender, PrimeNumberNotDetectedEventArgs e)
        {
            
        }

        // Inform: prime number detected
        private void primeNumberDetected(object? sender, EventArgs e)
        {
            
        }
    }

}