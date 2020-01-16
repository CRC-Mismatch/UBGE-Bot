using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    public sealed class Censo
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("Timestamp")]
        public object timestamp { get; set; }

        [BsonElement("Email")]
        public string emailMembro { get; set; }

        [BsonElement("ChegouAUBGE")]
        public string chegouNaUBGE { get; set; }

        [BsonElement("NomeDiscord"), BsonRepresentation(BsonType.String)]
        public ulong idNoDiscord { get; set; }

        [BsonElement("Idade")]
        public object idade { get; set; }

        [BsonElement("JogosMaisJogados")]
        public string jogosMaisJogados { get; set; }

        [BsonElement("Estado")]
        public string estado { get; set; }

        [BsonElement("Idiomas")]
        public string idiomas { get; set; }

        [BsonElement("FezCenso")]
        public bool fezOCenso { get; set; }
    }
}