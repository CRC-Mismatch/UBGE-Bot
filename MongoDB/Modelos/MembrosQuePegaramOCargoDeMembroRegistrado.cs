using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class MembrosQuePegaramOCargoDeMembroRegistrado
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement, BsonRepresentation(BsonType.String)]
        public int NumeroPessoasQuePegaramOCargoDeMembroRegistrado { get; set; }

        [BsonElement]
        public List<ulong> IdsMembrosQuePegaramOCargo { get; set; }
    }
}
