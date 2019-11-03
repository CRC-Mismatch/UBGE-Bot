using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    [BsonIgnoreExtraElements]
    public sealed class Jogos
    {
        [BsonId]
        public ObjectId id_ { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong CargoID { get; set; }

        [BsonElement]
        public string NomeDaCategoria { get; set; }


        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong IdDoEmoji { get; set; }
    }
}