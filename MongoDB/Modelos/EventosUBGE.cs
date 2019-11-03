using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class EventosUBGE
    {
        [BsonElement]
        public ObjectId _id;

        [BsonElement]
        public string NomeEvento { get; set; }

        [BsonElement]
        public List<string> TimesDoEvento { get; set; }

        [BsonElement]
        public List<string> JogadoresDoEvento { get; set; }

        [BsonElement]
        public List<string> JogadoresReservasDoEvento { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public int LimiteTimes { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public int LimiteJogadores { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public int LimiteJogadoresReservas { get; set; }
    }
}
