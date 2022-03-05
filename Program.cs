using System.Net;
using System.Net.Sockets;
using dc.assignment.primenumbers.utils;
using dc.assignment.primenumbers.models;

namespace dc.assignment.primenumbers{
    class Program{

        public static KLogger logger;

        static void Main(string[] args)
        {
            Program.logger = new KLogger();

            KTCPListener tcpl = new KTCPListener("127.0.0.1",5050);
            tcpl.onClientRequest += processClientRequest;
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