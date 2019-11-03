using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class Censo
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement]
        public object Timestamp { get; set; }

        [BsonElement]
        public string Email { get; set; }

        [BsonElement]
        public string ChegouAUBGE { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong NomeDiscord { get; set; }

        [BsonElement]
        public object Idade { get; set; }

        [BsonElement]
        public string JogosMaisJogados { get; set; }

        [BsonElement]
        public string Estado { get; set; }

        [BsonElement]
        public string Idiomas { get; set; }

        [BsonElement]
        public bool FezCenso { get; set; }
    }
}