using System.Net;
using System.Net.Sockets;
using dc.assignment.primenumbers.models;

namespace dc.assignment.primenumbers.utils{

    class KTCPListener{
        private TcpListener tcpListener; 
        private string ipAddress;
        private int port; 
        public event EventHandler<KTCPListenerEventArgs> onClientRequest;

        public KTCPListener(string ipAddress, int port){
            this.ipAddress = ipAddress;
            this.port = port;
            initHTTPServer();
        }

        private void initHTTPServer(){
            try  
            {  
                // start listing on the given port  
                tcpListener = new TcpListener(IPAddress.Parse(this.ipAddress),port);  
                tcpListener.Start();  
                Console.WriteLine("TCP Listener started.");

                // start the thread which calls the method 'StartListen'  
                Thread listenerThread = new Thread(
                    new ThreadStart(listenToHTTPRequest)
                );  
                listenerThread.Start();  
            }  
            catch (Exception e)  
            {  
                Console.WriteLine("An exception occurred in TCP Listener\n" + e.ToString());  
            }  
        }

        private void listenToHTTPRequest(){
            while (true){
                TcpClient tcpClient = tcpListener.AcceptTcpClient();  
                if (tcpClient.Connected)  {
                    Console.WriteLine("A client connected from " + tcpClient.Client.RemoteEndPoint.ToString()); 

                    NetworkStream stream = tcpClient.GetStream();
                    StreamReader reader = new StreamReader(stream);

                    byte[] bytes = new byte[tcpClient.SendBufferSize];
                    int recv = 0;
                    String received ="";
                    while (true)
                    {
                        recv = stream.Read(bytes, 0, tcpClient.SendBufferSize);
                        received += System.Text.Encoding.ASCII.GetString(bytes, 0, recv);

                        if (recv > 0 || recv ==0)
                        {
                            break;
                        }
                    }
                    Console.WriteLine("Data received:\n" + received);

                    // prepare custom request object 
                    KHTTPRequest request = new KHTTPRequest(received);
                    
                    // event call for request processing
                    onClientRequest?.Invoke(this, new KTCPListenerEventArgs(request, tcpClient));

                    // mandatory to close the client connection
                    tcpClient.Close();
                }
            }  
        }
    }

}