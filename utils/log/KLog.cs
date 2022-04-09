using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dc.assignment.primenumbers.utils.log
{
    class KLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string timestamp { get; set; } = null!;
        public Int64 nodeId { get; set; }
        public string nodeName { get; set; } = null!;
        public string message { get; set; } = null!;
    }
}