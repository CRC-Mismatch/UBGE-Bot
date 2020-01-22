using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace UBGE.MongoDB.Models
{
    public sealed class Reunion
    {
        [BsonId]
        public ObjectId _id;

        [BsonElement, BsonRepresentation(BsonType.String)]
        public DateTime DayOfReunion { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public DateTime LastDayToMarkThePresenceReaction { get; set; }

        [BsonElement]
        public string StaveOfReunion { get; set; }

        [BsonElement]
        public List<ulong> MemberWhoWillAttend { get; set; }
        
        [BsonElement]
        public List<ulong> MemberWhoWillNotAttend { get; set; }

        [BsonElement, BsonRepresentation(BsonType.String)]
        public ulong IdOfMessage { get; set; }

        [BsonElement]
        public bool ReunionIsFinished { get; set; }

        [BsonElement]
        public string LinkOfMessage { get; set; }
    }
}
