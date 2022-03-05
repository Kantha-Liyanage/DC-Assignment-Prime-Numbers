using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dc.assignment.primenumbers.utils.logger{
    class KLog{

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]        
        public string? Id { get; set; }
        public string timestamp { get; set; } = null!;
        [BsonElement("name")]
        public string node { get; set; } = null!;
        public string message { get; set; } = null!;
    }
}