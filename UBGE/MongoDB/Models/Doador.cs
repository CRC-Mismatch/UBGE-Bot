using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    public sealed class Doador
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong idDoMembro { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public List<string> diasEHorasQueOMembroVirouDoador { get; set; }

        [BsonElement]
        public string jaDoou { get; set; }
    }
}