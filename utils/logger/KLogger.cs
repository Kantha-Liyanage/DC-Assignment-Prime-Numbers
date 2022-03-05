using MongoDB.Driver;

namespace dc.assignment.primenumbers.utils.logger{

    class KLogger{
        IMongoCollection<KLog> _logsCollection;
        public KLogger(){
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var mongoDatabase = mongoClient.GetDatabase("sliit_dc_logs");
            _logsCollection = mongoDatabase.GetCollection<KLog>("logs");
        }

        public void log(Int64 nodeId, string nodeAddress, string message){
            KLog log = new KLog();
            log.nodeId = nodeId;
            log.nodeAddress = nodeAddress;
            log.timestamp = "now";
            log.message = message;
            Task task = logAsync(log);
            task.Wait();
        }

        private async Task logAsync(KLog log){
           await _logsCollection.InsertOneAsync(log);
        }
    }

}