using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGE_Bot.MongoDB.Modelos
{
    public sealed class EventosUBGE
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement("NomeEvento")]
        public string nomeDoEvento { get; set; }

        [BsonElement("TimesDoEvento")]
        public List<string> timesDoEvento { get; set; }

        [BsonElement("JogadoresDoEvento")]
        public List<string> jogadoresDoEvento { get; set; }

        [BsonElement("JogadoresReservasDoEvento")]
        public List<string> jogadoresReservasDoEvento { get; set; }

        [BsonElement("LimiteTimes"), BsonRepresentation(BsonType.String)]
        public int limiteDeTimes { get; set; }

        [BsonElement("LimiteJogadores"), BsonRepresentation(BsonType.String)]
        public int limiteDeJogadores { get; set; }

        [BsonElement("LimiteJogadoresReservas"), BsonRepresentation(BsonType.String)]
        public int limiteDeJogadoresReservas { get; set; }
    }
}
