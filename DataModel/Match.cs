namespace DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Runtime.Serialization;

    [DataContract]
    public class Match
    {
        [DataMember]
        public Guid MeetId { get; set; }

        [DataMember]
        public Guid UserId { get; set; }
    }
}
