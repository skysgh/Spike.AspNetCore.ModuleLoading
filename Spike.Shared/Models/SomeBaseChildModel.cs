using System.Runtime.Serialization;

namespace App.Base.Shared.Models
{
    [DataContract(Name ="Child")]
    public class SomeBaseChildModel
    {


        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int ParentFK { get; set; }
        [DataMember]
        public string Street { get; set; }
    }

}
