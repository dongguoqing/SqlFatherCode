namespace Daemon.Model.ViewModel
{
    using System.Collections.Generic;
    public class BaseUser
    {
        public string UserName { get; set; }

        public string UserAccount { get; set; }

        public int Id { get; set; }

        public List<int> IdList { get; set; }

        public string PassWord { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }

        public string Avatar { get; set; }

        public string Token { get; set; }
    }
}
