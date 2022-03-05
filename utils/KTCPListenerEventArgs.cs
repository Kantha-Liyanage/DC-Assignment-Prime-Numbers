using System.Net.Sockets;
using dc.assignment.primenumbers.models;

namespace dc.assignment.primenumbers.utils{

    class KTCPListenerEventArgs : EventArgs {

        public KHTTPRequest request { get; } 
        public TcpClient tcpClient { get; }

        public KTCPListenerEventArgs(KHTTPRequest request, TcpClient tcpClient){
            this.request = request;
            this.tcpClient = tcpClient;
        }

    }

}