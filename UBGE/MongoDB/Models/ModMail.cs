using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    public sealed class ModMail
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong idDoMembro { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong idDoCanal { get; set; }

        [BsonElement]
        public Denuncia denuncia { get; set; }

        [BsonElement]
        public Sugestao sugestao { get; set; }

        [BsonElement]
        public Contato contato { get; set; }
    }

    public sealed class Denuncia
    {
        [BsonElement]
        public string denunciaDoMembro { get; set; }

        [BsonElement]
        public string motivoDaDenunciaDoMembro { get; set; }

        [BsonElement]
        public string diaHoraDenuncia { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public bool oCanalFoiFechado { get; set; }
    }

    public sealed class Sugestao
    {
        [BsonElement]
        public string sugestaoDoMembro { get; set; }

        [BsonElement]
        public string diaHoraSugestao { get; set; }
    }

    public sealed class Contato
    {
        [BsonElement]
        public string diaHoraContato { get; set; }

        [BsonElement]
        public string motivoDoContatoDoMembro { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public bool oCanalFoiFechado { get; set; }
    }
}