using System;
using System.Text.Json;
using dc.assignment.primenumbers.dto;
using dc.assignment.primenumbers.utils.tcplistener;

namespace dc.assignment.primenumbers.models
{
    public class NumbersFileHandler
    {
        private string inputFile;
        private string outputFile;
        private string[] fileLines;
        private int currentNumberPosition;
        private int currentNumber;
        KTCPListener tcpListener;
        public NumbersFileHandler(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;

            tcpListener = new KTCPListener("127.0.0.1", 8282);
            this.tcpListener.onClientRequest += handleRequests;

            // init
            this.fileLines = System.IO.File.ReadAllLines(inputFile);
            this.currentNumberPosition = -1;
            this.currentNumber = 0;
        }

        private void handleRequests(object? sender, KTCPListenerEventArgs e)
        {
            if (e.request.resourceURL.Equals("getNextNumber") && e.request.httpMethod == KHTTPMethod.GET)
            {
                int nextNumber = this.getNextNumber();
                Console.WriteLine(nextNumber);
                KHTTPResponse response = new KHTTPResponse(HTTPResponseCode.OK_200, new { number = nextNumber });
                response.sendJSON(e.tcpClient);
            }
            else if (e.request.resourceURL.Equals("completeNumber") && e.request.httpMethod == KHTTPMethod.POST)
            {
                PrimeResultDTO? dto = JsonSerializer.Deserialize<PrimeResultDTO>(e.request.bodyContent);
                this.completeNumber(dto.number, dto.isPrime, dto.divisibleByNumber);
                KHTTPResponse response = new KHTTPResponse(HTTPResponseCode.OK_200, new { message = "Successful." });
                response.sendJSON(e.tcpClient);
                Console.WriteLine(dto.number);
            }
        }

        private int getNextNumber()
        {
            // some basic validations...
            // no numbers in the file
            if (this.fileLines == null) { return -1; }

            // last number still pending
            if (this.currentNumber != 0) { return this.currentNumber; }

            // next number position
            currentNumberPosition++;
            // eof
            if (currentNumberPosition >= fileLines.Length) { return -1; }

            // get next number
            this.currentNumber = int.Parse(fileLines[currentNumberPosition]);
            return this.currentNumber;
        }

        private void completeNumber(int theNumber, bool isPrime, int divisibleByNumber)
        {
            this.currentNumber = 0;
            System.IO.File.AppendAllText(this.outputFile, theNumber.ToString() + ":" + isPrime.ToString() + ":" + divisibleByNumber + "\n");
        }
    }
}