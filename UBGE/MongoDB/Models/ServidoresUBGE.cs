using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UBGE.MongoDB.Models
{
    public sealed class ServidoresUBGE
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("Jogo")]
        public string jogo { get; set; }

        [BsonElement("Jogadores")]
        public string jogadoresDoServidor { get; set; }

        [BsonElement("Mapa")]
        public string mapaDoServidor { get; set; }

        [BsonElement("MaximoPlayers")]
        public string maximoDePlayers { get; set; }

        [BsonElement("ModoDeJogo")]
        public string modoDeJogo { get; set; }

        [BsonElement("Pais")]
        public string paisDoServidor { get; set; }

        [BsonElement("Versao")]
        public string versaoDoJogo { get; set; }

        [BsonElement("Nome")]
        public string nomeDoServidor { get; set; }

        [BsonElement("Foto")]
        public string fotoDoServidor { get; set; }

        [BsonElement("Ip")]
        public string ipDoServidor { get; set; }

        [BsonElement("Porta")]
        public string portaDoServidor { get; set; }

        [BsonElement("Status")]
        public string statusDoServidor { get; set; }

        [BsonElement("FotoThumbnail")]
        public string thumbnailDoServidor { get; set; }

        [BsonElement("ServidorDisponivel")]
        public string servidorDisponivel { get; set; }

        [BsonElement("NomeServidorComando")]
        public string nomeServidorParaComando { get; set; }
    }
}