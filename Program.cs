using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace HelloWorld
{
    class Program
    {
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
                    
                    // method 1
                    //byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes("{\"msg\":\"Hurray!\"}"); 				
                    //send200Response(tcpClient, serverMessageAsByteArray);
                    
                    // method 2
                    var msg = new {
                        message = "hurray! Simplified"
                    };	
                    send200Response(tcpClient, msg);
                    
                    tcpClient.Close();
                }
            }  
        }

        private void getHTTPMethod(){}

        private void getHTTPRequestURL(){}

        private void send200Response(TcpClient tcpClient, byte[] payload){
            StringBuilder header = new StringBuilder();
            header.Append("HTTP/1.1 200 OK\r\n");
            header.Append("Content-Type: application/json\r\n");
            header.Append("Content-Length: " + payload.Length + "\r\n\n");
            byte[] headerBytes = Encoding.ASCII.GetBytes(header.ToString());

            tcpClient.GetStream().Write(headerBytes, 0, headerBytes.Length);
            tcpClient.GetStream().Write(payload, 0, payload.Length);
        }

        private void send200Response(TcpClient tcpClient, Object obj){
            string jsonString = JsonSerializer.Serialize(obj);
            byte[] payload = Encoding.ASCII.GetBytes(jsonString);

            StringBuilder header = new StringBuilder();
            header.Append("HTTP/1.1 200 OK\r\n");
            header.Append("Content-Type: application/json\r\n");
            header.Append("Content-Length: " + payload.Length + "\r\n\n");
            byte[] headerBytes = Encoding.ASCII.GetBytes(header.ToString());

            tcpClient.GetStream().Write(headerBytes, 0, headerBytes.Length);
            tcpClient.GetStream().Write(payload, 0, payload.Length);
        }
    }
}