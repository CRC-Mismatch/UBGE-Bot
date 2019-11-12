using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGE_Bot.MongoDB.Modelos
{
    public sealed class Prisioneiro
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("id"), BsonRepresentation(BsonType.String)]
        public ulong idDoMembro { get; set; }

        [BsonElement("roles"), BsonRepresentation(BsonType.String)]
        public List<ulong> cargosDoMembro { get; set; }
    }
}