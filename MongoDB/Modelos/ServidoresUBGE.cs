using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGEBot.MongoDB.Modelos
{
    public sealed class ServidoresUBGE
    {
        [BsonElement("Jogo")]
        public string Jogo { get; set; }

        [BsonElement("Jogadores")]
        public string Jogadores { get; set; }

        [BsonElement("Mapa")]
        public string Mapa { get; set; }

        [BsonElement("MaximoPlayers")]
        public string MaximoPlayers { get; set; }

        [BsonElement("ModoDeJogo")]
        public string ModoDeJogo { get; set; }

        [BsonElement("Pais")]
        public string Pais { get; set; }

        [BsonElement("Versao")]
        public string Versao { get; set; }

        [BsonElement("Nome")]
        public string Nome { get; set; }

        [BsonElement("Foto")]
        public string Foto { get; set; }

        [BsonElement("Ip")]
        public string Ip { get; set; }

        [BsonElement("Porta")]
        public string Porta { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("FotoThumbnail")]
        public string FotoThumbnail { get; set; }

        [BsonId]
        public ObjectId ID { get; set; }
    }
}