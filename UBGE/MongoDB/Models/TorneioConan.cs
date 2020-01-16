using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    public sealed class TorneioConan
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement("IdMembro")]
        public object idDoMembro { get; set; }

        [BsonElement("NickMembro")]
        public string nickDoMembro { get; set; }
    }
}