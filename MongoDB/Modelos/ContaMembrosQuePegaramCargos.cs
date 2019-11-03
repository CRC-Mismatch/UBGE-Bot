using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class ContaMembrosQuePegaramCargos
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement]
        public string Jogo { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public int NumeroPessoas { get; set; }

        [BsonElement]
        public List<ulong> IdsMembrosQuePegaramOCargo { get; set; }
    }
}
