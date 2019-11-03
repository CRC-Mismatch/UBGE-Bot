using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class Levels
    {
        [BsonElement]
        public ObjectId _id { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong IdMembro { get; set; }
        
        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong XP { get; set; }

        [BsonElement]
        public string NomeLevel { get; set; }

        [BsonElement]
        public string DiaEHora { get; set; }
    }
}
