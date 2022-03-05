using dc.assignment.primenumbers.models;
using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers{
    public class AppNode {
        public Int64 id {get;}
        private string ipAddress;
        private int port;
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
        }

        public string getAddress(){
            return this.ipAddress + ":" + this.port;
        }

        private void processClientRequest(object? sender, KTCPListenerEventArgs e)
        {
            KHTTPResponse reponse;

            switch(e.request.resourceURL){
                case "check": 
                    reponse = handleRequestCheck(e.request.urlParams);
                    break;
                case "abort":
                    reponse = handleRequestAbort();
                    break;
                default: 
                    reponse = new KHTTPResponse(HTTPResponseCode.Not_Found_404, new { message = "Resource not found" });
                    break;
            }

            // send response
            reponse.send(e.tcpClient);
        }

        private KHTTPResponse handleRequestCheck(List<KeyValuePair<string, string>> urlParams)
        {
            if(this.primeNumberChecker != null){
                if(this.primeNumberChecker.isChecking()){
                    return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. A number is being chekced currently." });
                }
            }

            // urlParams Ex: number=12345&from=0&to=5000
            try{
                string theNumberValue = urlParams.Find(x=>(x.Key.Equals("number"))).Value;
                int theNumber = int.Parse(theNumberValue);
                string fromValue = urlParams.Find(x=>(x.Key.Equals("from"))).Value;
                int fromNumber = int.Parse(fromValue);
                string toValue = urlParams.Find(x=>(x.Key.Equals("to"))).Value;
                int toNumber = int.Parse(toValue);

                this.primeNumberChecker = new PrimeNumberChecker(theNumber,fromNumber,toNumber);
                bool accepted = this.primeNumberChecker.start();
                if(accepted){
                    this.primeNumberChecker.onPrimeNumberDetected += primeNumberDetected;
                    this.primeNumberChecker.onPrimeNumberNotDetected += primeNumberNotDetected;

                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Accepted" });
                }
                else{
                    return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. Invalid input." });
                }
            }
            catch(Exception er){
                return new KHTTPResponse(HTTPResponseCode.Not_Acceptable_406, new { message = "Not accepted. Invalid input." });
            }
        }

        private KHTTPResponse handleRequestAbort()
        {
            if(this.primeNumberChecker != null){
                if(this.primeNumberChecker.isChecking()){
                    this.primeNumberChecker.abort();
                    return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Checking aborted." });
                }
            }

            return new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Already idle."});
        }

        private void primeNumberNotDetected(object? sender, PrimeNumberNotDetectedEventArgs e)
        {
            
        }

        private void primeNumberDetected(object? sender, EventArgs e)
        {
            
        }
    }

}