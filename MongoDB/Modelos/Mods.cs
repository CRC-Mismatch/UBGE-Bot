using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class Mods
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement]
        public string NomeDoMod { get; set; }
    }
}