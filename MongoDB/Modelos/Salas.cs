using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class Salas
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement]
        public string NomeSala { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong DonoId { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public int LimiteUsuarios { get; set; }

        [BsonElement]
        public bool Trancada { get; set; }

        [BsonElement]
        public List<ulong> IdsPermitidos { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong IdSala { get; set; }
    }
}