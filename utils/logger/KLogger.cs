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
        public KLogger(bool logViewServer)
        {
            // database
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var mongoDatabase = mongoClient.GetDatabase("sliit_dc_logs");
            _logsCollection = mongoDatabase.GetCollection<KLog>("logs");

            // web view
            if (logViewServer)
            {
                tcpListener = new KTCPListener("127.0.0.1", 8181);
                this.tcpListener.onClientRequest += handleRequests;
            }
        }

        public void log(Int64 nodeId, string nodeAddress, string message)
        {
            KLog log = new KLog();
            log.nodeId = nodeId;
            log.nodeAddress = nodeAddress;
            log.timestamp = "now";
            log.message = message;
            Task task = logAsync(log);
            task.Wait();
        }

        private void handleRequests(object? sender, KTCPListenerEventArgs e)
        {
            if (e.request.resourceURL.Equals("Logs") && e.request.httpMethod == HTTPMethod.GET)
            {
                // API: log entries
                List<KLog> array = getLogs();
                KHTTPResponse response = new KHTTPResponse(HTTPResponseCode.OK_200, array);
                response.sendJSON(e.tcpClient);
            }
            else if (e.request.resourceURL.Equals("Index"))
            {
                // HTML Page: Log entries
                // Grid control taken from https://gridjs.io/
                string html =
                @"<html>
                    <head>
                        <title>Prime Numbers - Distributed System - Logs</title>
                        <script src='https://unpkg.com/jquery/dist/jquery.min.js'></script>
                        <script src='https://unpkg.com/gridjs-jquery/dist/gridjs.production.min.js'></script>
                        <link rel='stylesheet' type='text/css' href='https://unpkg.com/gridjs/dist/theme/mermaid.min.css'/>
                        <body>
                            <div id='wrapper'></div>
                            <script>
                                $.get('http://localhost:8181/Logs', function(dataJSON, status){
                                    //data received
                                    $('div#wrapper').Grid({
                                        search: true,
                                        sort: true,
                                        autoWidth: true,
                                        pagination: false,
                                        columns: [
                                            {id: 'Id', name: 'Log Id'},
                                            {id: 'timestamp', name: 'Timestamp'},
                                            {id: 'nodeId', name: 'Node Id'},
                                            {id: 'nodeAddress', name: 'Node Address'},
                                            {id: 'message', name: 'Message'}
                                        ],
                                        data: dataJSON,
                                        style: {
                                            table: {
                                                'border': '3px solid #ccc',
                                                'font-family':'Courier New'
                                            },
                                            th: {
                                                'background-color': 'rgba(0, 0, 0, 0.1)',
                                                'color': '#000',
                                                'border-bottom': '3px solid #ccc',
                                                'text-align': 'center',
                                                'font-size': 12,
                                                'font-weight': 'bold'
                                            },
                                            td: {
                                                'text-align': 'left',
                                                'font-size': 12,
                                            }
                                        }
                                    });
                                });
                                </script>
                        </body>
                    <head>
                </html>";
                KHTTPResponse response = new KHTTPResponse(html);
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