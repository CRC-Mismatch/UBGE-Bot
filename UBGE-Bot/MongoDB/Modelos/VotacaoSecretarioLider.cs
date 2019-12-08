using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace UBGE_Bot.MongoDB.Modelos
{
    public sealed class VotacaoSecretarioLider
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong idDaMensagemDaVotacao { get; set; }

        [BsonElement]
        public string vencedorVotacao { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public DateTime proximaVotacao { get; set; }
    }
}
