namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.Generic;
    public partial class ArticleComment
    {
        [NotMapped]
        public List<ArticleComment> ApplyComment { get; set; }

        [ForeignKey("ToId")]
        public BlogUser ToUser{get;set;}
    }
}
