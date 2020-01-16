using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    [BsonIgnoreExtraElements]
    public sealed class Jogos
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("CargoID"), BsonRepresentation(BsonType.String)]
        public ulong idDoCargo { get; set; }

        [BsonElement("NomeDaCategoria")]
        public string nomeDaCategoria { get; set; }

        [BsonElement("IdDoEmoji"), BsonRepresentation(BsonType.String)]
        public ulong idDoEmoji { get; set; }
    }
}