using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGEBot.MongoDB.Modelos
{
    public class Infracao
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement]
        public object dataInfracao { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong idInfrator { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong idStaff { get; set; }

        [BsonElement]
        public string motivoInfracao { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public bool preso { get; set; }

        [BsonElement]
        public LogPrisao dadosPrisao { get; set; }
    }

    public sealed class LogPrisao
    {
        [BsonElement]
        public string tempo { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public List<ulong?> cargos { get; set; }
    }
}