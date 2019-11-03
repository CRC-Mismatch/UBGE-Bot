using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class TorneioConan
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement]
        public object IdMembro { get; set; }

        [BsonElement]
        public string NickMembro { get; set; }
    }
}