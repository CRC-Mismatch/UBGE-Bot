using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace UBGE.MongoDB.Models
{
    public sealed class CheckBotAberto
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("Numero"), BsonRepresentation(BsonType.String)]
        public int numero { get; set; }

        [BsonElement("DiaHora"), BsonRepresentation(BsonType.String)]
        public DateTime diaEHora { get; set; }
    }
}