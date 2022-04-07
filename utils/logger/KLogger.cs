using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using dc.assignment.primenumbers.utils.tcplistener;
using MongoDB.Bson;
using MongoDB.Driver;

namespace dc.assignment.primenumbers.utils.logger
{
    class KLogger
    {
        IMongoCollection<KLog> _logsCollection;
        KTCPListener tcpListener;
        public KLogger(bool logViewServerMode)
        {
            // database
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var mongoDatabase = mongoClient.GetDatabase("sliit_dc_logs");
            _logsCollection = mongoDatabase.GetCollection<KLog>("logs");

            // web view
            if (logViewServerMode)
            {
                tcpListener = new KTCPListener("127.0.0.1", 8181);
                this.tcpListener.onClientRequest += handleRequests;

                // clear all old logs
                _logsCollection.DeleteMany(new BsonDocument());
            }
        }

        public void log(Int64 nodeId, string nodeName, string message)
        {
            KLog log = new KLog();
            log.nodeId = nodeId;
            log.nodeName = nodeName;
            log.timestamp = DateTime.Now.Ticks + "";
            log.message = message;
            Task task = logAsync(log);
            task.Wait();
        }

        private void handleRequests(object? sender, KTCPListenerEventArgs e)
        {
            if (e.request.resourceURL.Equals("Logs") && e.request.httpMethod == KHTTPMethod.GET)
            {
                // API: log entries
                List<KLog> array = getLogs();
                KHTTPResponse response = new KHTTPResponse(HTTPResponseCode.OK_200, array);
                response.sendJSON(e.tcpClient);
            }
            else if (e.request.resourceURL.Equals("Index"))
            {
                // HTML Page: Log entries
                string pageHTML = System.IO.File.ReadAllText("index.html");
                KHTTPResponse response = new KHTTPResponse(pageHTML);
                response.sendHTML(e.tcpClient);
            }
        }

        private async Task logAsync(KLog log)
        {
            await _logsCollection.InsertOneAsync(log);
        }

        private List<KLog> getLogs()
        {
            List<KLog> logs = _logsCollection.Find(new BsonDocument()).ToList();
            return logs;
        }
    }
}