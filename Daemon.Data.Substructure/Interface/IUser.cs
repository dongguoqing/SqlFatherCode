namespace Daemon.Data.Substructure.Interface
{
    using System.Collections.Generic;
    public interface IUser
    {
        int Id { get; set; }

        int UserID { get; }

        bool Administrator { get; }

        string LoginID { get; }

        byte[] Password { get; }

        string FirstName { get; }

        string LastName { get; }

        IEnumerable<IRole> Roles_Public { get; }

        string FirstInitialLastName { get; }
    }
}
