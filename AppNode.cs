using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers{
    public class AppNode {
        public int id {get;}
        private string ipAddress;
        private int port;
        KTCPListener tcpListener;
        public AppNode(string ipAddress, int port){
            this.ipAddress = ipAddress;
            this.port = port;
            this.tcpListener = new KTCPListener(this.ipAddress,this.port);
            this.tcpListener.onClientRequest += processClientRequest;
        }

        public string toString(){
            return "AppNode [" + id + "] is running on " + this.ipAddress + ":" + this.port; 
        }

        private static void processClientRequest(object? sender, KTCPListenerEventArgs e)
        {
            var msg = new {
                message = e.request.resourceURL
            };	
                    
            KHTTPResponse reponse = new KHTTPResponse(HTTPResponseCode.OK_200, msg);
            reponse.send(e.tcpClient);
        }
    }

}