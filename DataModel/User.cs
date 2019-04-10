namespace DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Runtime.Serialization;

    [DataContract]
    [Table("User")]
    public partial class User
    {
        public User()
        {
            CreatedMeets = new HashSet<Meet>();
            TakenMeets = new HashSet<Meet>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [DataMember]
        [Required]
        [StringLength(50)]
        public string Password { get; set; }

        [DataMember]
        public virtual ICollection<Meet> CreatedMeets { get; set; }

        [DataMember]
        public virtual ICollection<Meet> TakenMeets { get; set; }
    }
}
