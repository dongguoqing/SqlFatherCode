using System.Text.Json.Serialization;
namespace Daemon.Model.Entities
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }

        public AuthenticateResponse(BlogUser user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            UserName = user.UserName;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}
