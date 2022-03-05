using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers{
    public class AppNode {
        public Int64 id {get;}
        private string ipAddress;
        private int port;
        KTCPListener tcpListener;
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

        private static void processClientRequest(object? sender, KTCPListenerEventArgs e)
        {
            var msg = new {
                message = e.request.resourceURL
            };	
                    
            KHTTPResponse reponse = new KHTTPResponse(HTTPResponseCode.OK_200, msg);
            reponse.send(e.tcpClient);
            Console.WriteLine("HTTP Status 200 returned.");
        }
    }

}