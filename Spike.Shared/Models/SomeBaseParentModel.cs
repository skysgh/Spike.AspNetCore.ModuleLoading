using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace App.Base.Shared.Models
{
    [DataContract(Name ="Parent")]
    public class SomeBaseParentModel
    {

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ICollection<SomeBaseChildModel> Addresses
        {
            get => _children;
            set => _children = value;
        }
        ICollection<SomeBaseChildModel> _children = new Collection<SomeBaseChildModel>();
    }

}
