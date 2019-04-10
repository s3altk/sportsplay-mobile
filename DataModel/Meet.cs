namespace DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Runtime.Serialization;

    [DataContract]
    [Table("Meet")]
    public partial class Meet
    {
        public Meet()
        {
            Partakers = new HashSet<User>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public Guid FounderId { get; set; }

        [DataMember]
        public Guid PlaygroundId { get; set; }

        [DataMember]
        public virtual Playground Playground { get; set; }

        [DataMember]
        public virtual User Founder { get; set; }

        [DataMember]
        public virtual ICollection<User> Partakers { get; set; }
    }
}
