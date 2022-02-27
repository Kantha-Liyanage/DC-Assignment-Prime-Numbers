using System.Net;
using System.Net.Sockets;
using dc.assignment.primenumbers.models;

namespace dc.assignment.primenumbers{
    class Program{
        static void Main(string[] args)
        {
            new AppNode("127.0.0.1",5050);
            new AppNode("127.0.0.1",5051);
        }
    }

    class AppNode{
        private TcpListener tcpListener; 
        private string ip;
        private int port; 
         
        public AppNode(string ip, int port){
            this.ip = ip;
            this.port = port;
            initHTTPServer();
        }

        private void initHTTPServer(){
            try  
            {  
                //start listing on the given port  
                tcpListener = new TcpListener(IPAddress.Parse(this.ip),port);  
                tcpListener.Start();  
                Console.WriteLine("AppNode Web Server:" + this.port + " Started!");  
                //start the thread which calls the method 'StartListen'  
                Thread listenerThread = new Thread(
                    new ThreadStart(listenToHTTPRequest)
                );  
                listenerThread.Start();  
            }  
            catch (Exception e)  
            {  
                Console.WriteLine("An Exception Occurred: " + e.ToString());  
            }  
        }

        private void listenToHTTPRequest(){
            while (true){
                TcpClient tcpClient = tcpListener.AcceptTcpClient();  
                if (tcpClient.Connected)  {
                    Console.WriteLine("Client Connected from " + tcpClient.Client.RemoteEndPoint.ToString()); 

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
                    Console.WriteLine("Received:\n" + received);

                    KHTTPRequest request = new KHTTPRequest(received);
                    
                    processRequest(request, tcpClient);
                    
                    tcpClient.Close();
                }
            }  
        }

        private void processRequest(KHTTPRequest request, TcpClient tcpClient){
            var msg = new {
                message = request.resourceURL
            };	
                    
            KHTTPResponse reponse = new KHTTPResponse(HTTPResponseCode.OK_200, msg);
            
            reponse.send(tcpClient);
        }
    }
}