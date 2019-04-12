using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WebMaker.Entity.Models
{
    [MetadataType(typeof(mgt_UserRoleRelationMetaData))]
    public partial class mgt_UserRoleRelation
    {
        [DataContract]
        public partial class mgt_UserRoleRelationMetaData
        {
            [DataMember]
            public Guid ID { get; set; }

            [DataMember]
            public Guid UserID { get; set; }

            [DataMember]
            public Guid RoleID { get; set; }

            [DataMember]
            public string RoleNumber { get; set; }    
                  
            [DataMember]
            public bool IsTimeLimited { get; set; }

            [DataMember]
            public DateTime? StartTime { get; set; }

            [DataMember]
            public DateTime? EndTime { get; set; }

            [DataMember]
            public DateTime CreateTime { get; set; }

            [DataMember]
            public Guid? OrderID { get; set; }

            [DataMember]
            public bool IsEnabled { get; set; }

            [DataMember]
            public bool IsDelete { get; set; }
        }
    }
}
