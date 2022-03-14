using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace dc.assignment.primenumbers.utils.tcplistener
{
    class KHTTPResponse
    {
        private HTTPResponseCode code;
        private Object payload;
        public KHTTPResponse(HTTPResponseCode code, Object payload)
        {
            this.code = code;
            this.payload = payload;
        }

        public KHTTPResponse(Object payload)
        {
            this.code = HTTPResponseCode.OK_200;
            this.payload = payload;
        }

        public void sendJSON(TcpClient? tcpClient)
        {
            string jsonString = JsonSerializer.Serialize(this.payload);
            byte[] payloadBytes = Encoding.ASCII.GetBytes(jsonString);

            StringBuilder header = new StringBuilder();
            switch (this.code)
            {
                case HTTPResponseCode.OK_200:
                    header.Append("HTTP/1.1 200 OK\r\n"); break;
                case HTTPResponseCode.Not_Found_404:
                    header.Append("HTTP/1.1 404 Not Found\r\n"); break;
                case HTTPResponseCode.Not_Acceptable_406:
                    header.Append("HTTP/1.1 406 Not Acceptable\r\n"); break;
                default:
                    header.Append("HTTP/1.1 500 Internal Server Error\r\n"); break;
            }

            header.Append("Content-Type: application/json\r\n");
            header.Append("Content-Length: " + payloadBytes.Length + "\r\n\n");
            byte[] headerBytes = Encoding.ASCII.GetBytes(header.ToString());

            tcpClient?.GetStream().Write(headerBytes, 0, headerBytes.Length);
            tcpClient?.GetStream().Write(payloadBytes, 0, payloadBytes.Length);

            Console.WriteLine("HTTP Status " + this.code + " returned.");
        }

        public void sendHTML(TcpClient? tcpClient)
        {
            byte[] payloadBytes = Encoding.ASCII.GetBytes(this.payload.ToString());

            StringBuilder header = new StringBuilder();
            header.Append("HTTP/1.1 200 OK\r\n");
            header.Append("Content-Type: text/html\r\n");
            header.Append("Content-Length: " + payloadBytes.Length + "\r\n\n");
            byte[] headerBytes = Encoding.ASCII.GetBytes(header.ToString());

            tcpClient?.GetStream().Write(headerBytes, 0, headerBytes.Length);
            tcpClient?.GetStream().Write(payloadBytes, 0, payloadBytes.Length);

            Console.WriteLine("HTTP Status " + this.code + " returned.");
        }
    }

    enum HTTPResponseCode
    {
        OK_200,
        Not_Found_404,
        Not_Acceptable_406,
        Internal_Server_Error_500
    }
}