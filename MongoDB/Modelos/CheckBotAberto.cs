using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class CheckBotAberto
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement, BsonRepresentation(BsonType.String)]
        public int Numero { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public DateTime DiaHora { get; set; }
    }
}