using BagiraWebApi.Configs.Auth;
using BagiraWebApi.Models.Auth;
using BagiraWebApi.Services.Auth.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace BagiraWebApi.Services.Auth
{
    public class AuthService
    {
        private AuthConfig _authConfig;
        private ApplicationContext _context;

        public AuthService(IOptions<AuthConfig> options, ApplicationContext context)
        {
            _authConfig = options.Value;
            _context = context;
        }

        public static TokenValidationParameters GetTokenValidationParameters(AuthConfig config, bool isRefresh = false)
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
                Encoding.UTF8.GetBytes(isRefresh ? config.RefreshKey : config.AccessKey))
            };
        }

        public async Task<AuthDto> SignIn(SignInForm signInForm)
        {
            var user = await _context.Users.SingleAsync(u => u.Email == signInForm.Email);

            if (!VerifyPassword(signInForm.Password, user.Password))
            {
                throw new Exception("Wrong password");
            }

            var session = await CreateSessionAsync(user);

            var tokens = GetTokens(session);

            return tokens;
        }

        public async Task<string> SignUpAsync(SignUpUserForm signUpUserForm)
        {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.Email == signUpUserForm.Email.ToLower());

            if (user != null)
            {
                throw new Exception($"User with email:{signUpUserForm.Email} already exist");
            }

            var createdUser = await _context.Users.AddAsync(new User
            {
                UserName = signUpUserForm.UserName,
                Email = signUpUserForm.Email.ToLower(),
                Password = HashPassword(signUpUserForm.Password)
            });

            await _context.SaveChangesAsync();

            return createdUser.Entity.Id.ToString();
        }

        public async Task<AuthDto> RefreshToken(string refreshToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenValidationResult = await handler.ValidateTokenAsync(
                refreshToken, GetTokenValidationParameters(_authConfig, true));

            if (!tokenValidationResult.IsValid)
            {
                throw new SecurityTokenValidationException();
            }

            var claims = tokenValidationResult.Claims;

            var sessionIdValue = claims.Single(claim => claim.Key == "sessionId").Value.ToString();
            var sessionId = Guid.Parse(sessionIdValue);
            var session = await _context.Sessions
                .Include(s => s.User)
                .SingleAsync(s => s.SessionId == sessionId);

            var tokenValue = claims.Single(claim => claim.Key == "token").Value.ToString();
            var sessionToken = Guid.Parse(tokenValue);

            if (session.SessionToken != sessionToken)
            {
                throw new SecurityTokenEncryptionFailedException();
            }

            session.SessionToken = Guid.NewGuid();
            session.Expires = DateTime.UtcNow.AddDays(_authConfig.RefreshTokenValidityInDays);

            var tokens = GetTokens(session);

            await _context.SaveChangesAsync();

            return tokens;
        }

        private string HashPassword(string password)
        {
            string passwordHash = BC.HashPassword(password);

            return passwordHash;
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BC.Verify(password, passwordHash);
        }

        private async Task<Session> CreateSessionAsync(User? user)
        {
            var session = await _context.Sessions.AddAsync(new Session
            {
                User = user,
                SessionToken = Guid.NewGuid(),
                Expires = DateTime.UtcNow.AddDays(_authConfig.RefreshTokenValidityInDays),
            });

            await _context.SaveChangesAsync();

            return session.Entity;
        }

        private AuthDto GetTokens(Session session)
        {
            var accessClaims = new List<Claim>();
            if (session.User != null)
            {
                accessClaims.Add(new(ClaimTypes.NameIdentifier, session.User.Id.ToString()));
            }

            var refreshClaims = new List<Claim>
            {
                new("sessionId", session.SessionId.ToString()),
                new("token", session.SessionToken.ToString())
            };

            var accessLifeTime = TimeSpan.FromMinutes(_authConfig.TokenValidityInMinutes);
            var refreshLifeTime = TimeSpan.FromDays(_authConfig.RefreshTokenValidityInDays);

            var accessToken = GetToken(accessClaims, _authConfig.AccessKey, accessLifeTime);
            var refreshToken = GetToken(refreshClaims, _authConfig.RefreshKey, refreshLifeTime);

            return new AuthDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = (int)accessLifeTime.TotalSeconds
            };
        }

        private string GetToken(List<Claim> claims, string key, TimeSpan lifeTime)
        {
            var jwt = new JwtSecurityToken(
                issuer: _authConfig.Issuer,
                audience: _authConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(lifeTime),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
