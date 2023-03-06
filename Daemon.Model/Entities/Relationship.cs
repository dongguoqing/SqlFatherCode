using System.Collections.Generic;
using System.Runtime.Serialization;
namespace Daemon.Model.Entities
{
    public partial class Relationship
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SelectFields { get; set; }

        [DataMember]
        public List<Relationship> Relationships { get; set; }

    }
}
