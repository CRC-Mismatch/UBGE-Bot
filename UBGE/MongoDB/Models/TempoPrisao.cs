using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    public sealed class TempoPrisao
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("IdMembro"), BsonRepresentation(BsonType.String)]
        public ulong idDoMembro { get; set; }

        [BsonElement("Tempo")]
        public string tempoPrisao { get; set; }
    }
}
