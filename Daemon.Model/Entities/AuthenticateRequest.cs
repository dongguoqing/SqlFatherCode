namespace Daemon.Model.Entities
{
    using System.ComponentModel.DataAnnotations;
    public class AuthenticateRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string PassWord { get; set; }
    }
}
