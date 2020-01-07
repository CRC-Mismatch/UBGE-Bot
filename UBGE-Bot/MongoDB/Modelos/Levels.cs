using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE_Bot.MongoDB.Modelos
{
    public sealed class Levels
    {
        [BsonElement]
        public ObjectId _id { get; set; }

        [BsonElement("IdMembro"), BsonRepresentation(BsonType.String)]
        public ulong idDoMembro { get; set; }

        [BsonElement("XP"), BsonRepresentation(BsonType.String)]
        public ulong xpDoMembro { get; set; }

        [BsonElement("NomeLevel")]
        public string nomeDoLevel { get; set; }

        [BsonElement("DiaEHora")]
        public string diaEHora { get; set; }
    }
}
