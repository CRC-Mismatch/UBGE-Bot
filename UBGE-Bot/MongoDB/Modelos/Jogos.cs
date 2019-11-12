using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE_Bot.MongoDB.Modelos
{
    [BsonIgnoreExtraElements]
    public sealed class Jogos
    {
        [BsonId]
        public ObjectId id_ { get; set; }

        [BsonElement("CargoID"), BsonRepresentation(BsonType.String)]
        public ulong idDoCargo { get; set; }

        [BsonElement("NomeDaCategoria")]
        public string nomeDaCategoria { get; set; }

        [BsonElement("IdDoEmoji"), BsonRepresentation(BsonType.String)]
        public ulong idDoEmoji { get; set; }
    }
}