using System.ComponentModel.DataAnnotations.Schema;
namespace Daemon.Model
{
    public partial class Article
    {
        [NotMapped]
        public string AddUserName { get; set; }

        [NotMapped]
        public string UpdateUserName { get; set; }

        [NotMapped]
        public string AuthorName { get; set; }
    }
}
