using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;
using Daemon.Model.ViewModel;
namespace Daemon.Model
{
    public partial class BlogUser 
    {
        [NotMapped]
        public string Code { get; set; }
        [NotMapped]
        public RefreshToken RefreshToken { get; set; }
        [NotMapped]
        public string Token { get; set; }
        [NotMapped]
        public string AddUserName { get; set; }
        [NotMapped]
        public string UpdateUserName { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
        [NotMapped]
        public List<int> IdList { get; set; }
        [NotMapped]
        public List<Resource> ResourceList { get; set; }
        [JsonIgnore]
        [NotMapped]
        public List<RefreshToken> RefreshTokens { get; set; }

        [NotMapped]
        [JsonIgnore]
        public bool AutoLogin { get; set; }
    }
}
