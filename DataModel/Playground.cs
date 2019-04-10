namespace DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Runtime.Serialization;

    [DataContract]
    [Table("Playground")]
    public partial class Playground
    {
        public Playground()
        {
            ArrangedMeets = new HashSet<Meet>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        [Required]
        [StringLength(100)]
        public string Address { get; set; }

        [DataMember]
        public double LocationX { get; set; }

        [DataMember]
        public double LocationY { get; set; }

        [DataMember]
        public virtual ICollection<Meet> ArrangedMeets { get; set; }
    }
}
