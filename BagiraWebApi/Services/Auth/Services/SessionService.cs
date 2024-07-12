using BagiraWebApi.Configs.Auth;
using BagiraWebApi.Exceptions.Auth;
using BagiraWebApi.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace BagiraWebApi.Services.Auth.Services
{
    public class SessionService
    {
        private readonly ApplicationContext _context;
        private readonly AuthConfig _authConfig;

        public SessionService(ApplicationContext context, AuthConfig authConfig)
        {
            _context = context;
            _authConfig = authConfig;
        }

        public async Task<Session> GetSessionAsync(string sessionId)
        {
            var sessionIdGuid = new Guid(sessionId);
            
            var session = await _context.Sessions.FirstOrDefaultAsync(x => x.SessionId == sessionIdGuid) 
                ?? throw new SessionNotFoundException();

            return session;
        }

        public void CheckSessionToken(Session session, string sessionToken)
        {
            var sessionTokenGuid = new Guid(sessionToken);

            if (session.SessionToken != sessionTokenGuid)
            {
                throw new SessionValidationException();
            }
        }

        public async Task<Session> CreateSessionAsync(User? user)
        {
            var session = await _context.Sessions.AddAsync(new Session
            {
                User = user,
                SessionToken = Guid.NewGuid(),
                Expires = GetSessionExpiresTime(),
            });

            await _context.SaveChangesAsync();

            return session.Entity;
        }

        public async Task RefreshSessionAsync(Session session)
        {
            session.SessionToken = Guid.NewGuid();
            session.Expires = GetSessionExpiresTime();

            await _context.SaveChangesAsync();
        }

        public async Task RemoveSessionAsync(Session session)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        }

        private DateTime GetSessionExpiresTime()
        {
            return DateTime.UtcNow.AddDays(_authConfig.RefreshTokenValidityInDays);
        }
    }
}
