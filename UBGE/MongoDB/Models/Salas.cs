using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGE.MongoDB.Models
{
    public sealed class Salas
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("NomeSala")]
        public string nomeDaSala { get; set; }

        [BsonElement("DonoId"), BsonRepresentation(BsonType.String)]
        public ulong idDoDono { get; set; }

        [BsonElement("LimiteUsuarios"), BsonRepresentation(BsonType.String)]
        public int limiteDeUsuarios { get; set; }

        [BsonElement("Trancada")]
        public bool salaTrancada { get; set; }

        [BsonElement("IdsPermitidos")]
        public List<ulong> idsPermitidos { get; set; }

        [BsonElement("IdSala"), BsonRepresentation(BsonType.String)]
        public ulong idDaSala { get; set; }

        [BsonElement("MembrosNaSala"), BsonRepresentation(BsonType.String)]
        public ulong membrosNaSala { get; set; }
    }
}