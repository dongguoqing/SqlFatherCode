using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Daemon.Model
{
    [Table("customer")]
    public class Customer
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public int CustomerRole { get; set; }

        public string RoleName { get; set; }

        public decimal Price { get; set; }

        public int PayOption { get; set; }

        public decimal AccountMoney { get; set; }
    }
}
