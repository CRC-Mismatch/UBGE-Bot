using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class Prisioneiro
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("id"), BsonRepresentation(BsonType.String)]
        public ulong MembroId { get; set; }

        [BsonElement("roles"), BsonRepresentation(BsonType.String)]
        public List<ulong> Cargos { get; set; }
    }
}