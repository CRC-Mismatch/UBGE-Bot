using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGE.MongoDB.Models
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

        [BsonElement("preso"), BsonRepresentation(BsonType.String)]
        public bool oMembroFoiPreso { get; set; }

        [BsonElement]
        public LogPrisao dadosPrisao { get; set; }
    }

    public sealed class LogPrisao
    {
        [BsonElement("tempo")]
        public string tempoDoMembroNaPrisao { get; set; }

        [BsonElement("cargos")]
        public List<ulong> cargosDoMembro { get; set; }
    }
}