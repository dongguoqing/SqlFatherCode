namespace Daemon
{
    using Daemon.Data.Infrastructure.Auth;
    using Microsoft.AspNetCore.Http;
    using Daemon.Common;
    using System.Linq;
    public static class ExtensionMethod
    {
    //  public static IAuthInfo GetAuthInfo(this HttpRequest request){
    //         var token = request.Query["token"].FirstOrDefault();
    //         if (string.IsNullOrEmpty(token))
    //             token = request.Headers["token"].FirstOrDefault();
    //         try
    //         {
    //             var userJson = JwtHelper.ValidateJwtToken(token, sign);
    //             var userInfo = JsonConvert.DeserializeObject<BlogUser>(userJson);
    //  }   
    }
}
