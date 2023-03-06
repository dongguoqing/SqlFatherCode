using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;
namespace Daemon.Model
{
    public partial class BlogRoleResource
    {
        public Resource Resource { get; set; }
    }
}
