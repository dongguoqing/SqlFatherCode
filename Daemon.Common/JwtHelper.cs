using System.ComponentModel;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Daemon.Model;
using System.Security.Cryptography;

namespace Daemon.Common
{
	public class JwtHelper
	{
		public static string CreateJwtToken(BlogUser user, string secret)
		{
			DateTime utcNow = DateTime.UtcNow;

			var claims = new List<Claim>() {
				new Claim("ID",user.Id.ToString()),
				new Claim("Name",user.UserName)
			};
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
			//A Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor that contains details of contents of the token.

			var tokenDescriptor = new JwtSecurityToken(issuer:"",audience:"",claims:claims,expires:DateTime.Now.AddDays(1),signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256));
			var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
			return token;
		}

		public static RefreshToken generateRefreshToken(string ipAddress)
		{
			using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
			{
				var randomBytes = new byte[64];
				rngCryptoServiceProvider.GetBytes(randomBytes);
				return new RefreshToken
				{
                    Token = Convert.ToBase64String(randomBytes),
					Expires = DateTime.UtcNow.AddDays(7),
					Created = DateTime.UtcNow,
					CreatedByIp = ipAddress
				};
			}
		}

		// public static string ValidateJwtToken(string token, string secret)
		// {
		// 	try
		// 	{
		// 		// IJsonSerializer serializer = new JsonNetSerializer();
		// 		// IDateTimeProvider provider = new UtcDateTimeProvider();
		// 		// IJwtValidator validator = new JwtValidator(serializer, provider);
		// 		// IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
		// 		// IJwtAlgorithm alg = new HMACSHA256Algorithm();
		// 		// IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, alg);
		// 		var jwtHandler = new JwtSecurityTokenHandler();
		// 		JwtSecurityToken jwtSecurityToken = jwtHandler.ReadJwtToken(token);
		// 		var json = decoder.Decode(token, secret, true);
		// 		return json;
		// 	}
		// 	catch (TokenExpiredException)
		// 	{
		// 		return "expired";
		// 	}
		// 	catch (SignatureVerificationException)
		// 	{
		// 		return "invalid";
		// 	}
		// 	catch (Exception)
		// 	{
		// 		return "error";
		// 	}
		// }


	}
}
