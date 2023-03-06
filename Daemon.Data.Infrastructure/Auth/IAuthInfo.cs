using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Daemon.Data.Substructure.Interface;
namespace Daemon.Data.Infrastructure.Auth
{
    public interface IAuthInfo
    {
        JObject AuthorizationTree { get; }

        bool IsAdmin { get; set; }

        bool IsSupportModeUser { get; }

        int UserId { get; set; }

        IUser UserEntity { get; }

        List<string> GetReports();

        List<string> GetSchools();

        bool IsAuthorizedFor(string permission, string operation);

        bool IsAuthorizedForDatasource(string databaseId);

        bool IsAuthorizedForReport(string repFilename, int type);

        bool IsFieldTripAdmin();

        bool IsAuthorizedForSchool(string schoolCode);

        bool IsAuthorizedForDataType(string dataType, string operation);

        bool IsAuthorizedForForm(int udgridId);

        string NormalizeSecureItem(string secureItemName);

        string NormalizeSecuredItemGroupName(string name);
    }
}
