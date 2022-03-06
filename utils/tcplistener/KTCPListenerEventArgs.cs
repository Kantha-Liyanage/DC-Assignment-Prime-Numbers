using System;
using System.Net.Sockets;

namespace dc.assignment.primenumbers.utils.tcplistener{

    class KTCPListenerEventArgs : EventArgs {

        public KHTTPRequest request { get; } 
        public TcpClient? tcpClient { get; }

        public KTCPListenerEventArgs(KHTTPRequest request, TcpClient? tcpClient){
            this.request = request;
            this.tcpClient = tcpClient;
        }

    }

}