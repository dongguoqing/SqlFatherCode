namespace Daemon.Model.Jwt
{
    using System;
    public class JwtAuthorizationDto
    {
        public long Auths { get; set; }

        public long Expires { get; set; }

        public bool Success { get; set; } = true;

        public string Token { get; set; }

        public int UserId { get; set; }
    }
}
