using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class Reacts
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement]
        public string Categoria { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong IdMensagens { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong Guild { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong CanalId { get; set; }
    }
}