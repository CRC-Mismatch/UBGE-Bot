using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class TempoPrisao
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong IdMembro { get; set; }

        [BsonElement]
        public string Tempo { get; set; }
    }
}
