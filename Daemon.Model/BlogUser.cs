using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Daemon.Model.ViewModel;
namespace Daemon.Model
{
    [Table("bloguser")]
    public partial class BlogUser : UserExt
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("UserName")]
        public string UserName { get; set; }

        [Column("PassWord")]
        public string PassWord { get; set; }

        [Column("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [Column("Avatar")]
        public string Avatar { get; set; }

        [Column("RealName")]
        public string RealName { get; set; }

        [Column("Email")]
        public string Email { get; set; }
        [Column("IsEnabled")]
        public int? IsEnabled { get; set; }

        [Column("Sex")]
        public int? Sex { get; set; }

        [Column("DeptId")]
        public int? DeptId { get; set; }

        [Column("DeptName")]
        public string DeptName { get; set; }

        [Column("AddTime")]
        public DateTime? AddTime { get; set; }

        [Column("UpdateTime")]
        public DateTime? UpdateTime { get; set; }
        [Column("IsAdmin")]
        public bool? IsAdmin { get; set; }
    }
}
