using BagiraWebApi.Configs.Auth;
using BagiraWebApi.Dtos.Auth;
using BagiraWebApi.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BagiraWebApi.Services.Auth.Services
{
    public class TokenService
    {
        private readonly AuthConfig _authConfig;

        public TokenService(AuthConfig authConfig)
        {
            _authConfig = authConfig;
        }

        public static TokenValidationParameters GetTokenValidationParameters(AuthConfig config)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = config.Issuer,
                ValidateAudience = true,
                ValidAudience = config.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config.Key))
            };
        }

        public TokensDto GetTokens(Session session)
        {
            var accessClaims = new List<Claim>();
            if (session.User != null)
            {
                accessClaims.Add(new("userId", session.User.Id.ToString()));
            }

            var refreshClaims = new List<Claim>
            {
                new("sessionId", session.SessionId.ToString()),
                new("sessionToken", session.SessionToken.ToString())
            };

            var accessLifeTime = TimeSpan.FromMinutes(_authConfig.TokenValidityInMinutes);
            var refreshLifeTime = TimeSpan.FromDays(_authConfig.RefreshTokenValidityInDays);

            var accessToken = GetToken(accessClaims, accessLifeTime);
            var refreshToken = GetToken(refreshClaims, refreshLifeTime);

            return new TokensDto
            {
                IsAnonymous = session.User == null,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = (int)accessLifeTime.TotalSeconds
            };
        }

        private string GetToken(List<Claim> claims, TimeSpan lifeTime)
        {
            var jwt = new JwtSecurityToken(
                issuer: _authConfig.Issuer,
                audience: _authConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(lifeTime),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_authConfig.Key)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
