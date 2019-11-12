using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGE_Bot.MongoDB.Modelos
{
    public sealed class MembrosQuePegaramOCargoDeMembroRegistrado
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement("NumeroPessoasQuePegaramOCargoDeMembroRegistrado"), BsonRepresentation(BsonType.String)]
        public int numeroPessoasQuePegaramOCargoDeMembroRegistrado { get; set; }

        [BsonElement("IdsMembrosQuePegaramOCargo")]
        public List<ulong> idsMembrosQuePegaramOCargo { get; set; }
    }
}
