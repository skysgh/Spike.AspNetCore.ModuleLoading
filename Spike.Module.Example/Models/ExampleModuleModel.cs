using App.Base.Shared.Models;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace App.Modules.Example.Models
{
    // This is a model introduced by 
    // the second EDM model
    // that in turn references a child
    // model from the first EDM...
    // Lets see if we can get it to work

    [DataContract(Name ="ExampleM")]
    public class ExampleModuleModel
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        // Collection of base module models
        // that don't know about this plugin
        // (probably would need an indrection object to pull 
        // it off in real life, but for now...)
        [DataMember]
        public ICollection<SomeBaseParentModel> SubChildren
        {
            get => _children;
            set => _children = value;
        }
        ICollection<SomeBaseParentModel> _children =
            new Collection<SomeBaseParentModel>();
    }
}
