using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    public sealed class Reacts
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("Categoria")]
        public string categoria { get; set; }

        [BsonElement("IdMensagens"), BsonRepresentation(BsonType.String)]
        public ulong idDaMensagem { get; set; }

        [BsonElement("Guild"), BsonRepresentation(BsonType.String)]
        public ulong servidor { get; set; }

        [BsonElement("CanalId"), BsonRepresentation(BsonType.String)]
        public ulong idDoCanal { get; set; }
    }
}