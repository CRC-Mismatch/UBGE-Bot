using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGE_Bot.MongoDB.Modelos
{
    public sealed class ContaMembrosQuePegaramCargos
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("Jogo")]
        public string jogo { get; set; }

        [BsonElement("NumeroPessoas"), BsonRepresentation(BsonType.String)]
        public int numeroDePessoas { get; set; }

        [BsonElement("IdsMembrosQuePegaramOCargo")]
        public List<ulong> idsDosMembrosQuePegaramOCargo { get; set; }
    }
}
