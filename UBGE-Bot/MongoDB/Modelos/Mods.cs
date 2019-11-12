using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE_Bot.MongoDB.Modelos
{
    public sealed class Mods
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement("NomeDoMod")]
        public string nomeDoMod { get; set; }
    }
}