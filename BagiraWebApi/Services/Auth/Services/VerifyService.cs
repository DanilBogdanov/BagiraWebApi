using BagiraWebApi.Configs.Auth;
using BagiraWebApi.Exceptions.Auth;
using BagiraWebApi.Models.Auth;
using BagiraWebApi.Services.Messengers;

namespace BagiraWebApi.Services.Auth.Services
{
    public class VerifyService
    {
        private readonly ApplicationContext _context;
        private readonly AuthConfig _authConfig;
        private readonly MessengerService _messengerService;

        public VerifyService(ApplicationContext applicationContext, AuthConfig authConfig, MessengerService messengerService)
        {
            _context = applicationContext;
            _authConfig = authConfig;
            _messengerService = messengerService;
        }

        public void CheckClientIsValid(string clientId, string clientSecret)
        {
            if (!_authConfig.Clients.Any(client => client.Id == clientId && client.Secret == clientSecret))
            {
                throw new ClientValidationException();
            }
        }

        public async Task CheckCode(User user, string code)
        {
            if (user.VerificationCode == null)
            {
                var ex = new SignInException
                {
                    NeedToRequestVerifyCode = true
                };
                throw ex;
            }

            if (user.VerificationCode == code)
            {
                user.VerificationCode = null;
                user.Confirmed = true;
                user.AccessFailedCount = 0;

                await _context.SaveChangesAsync();
            }
            else
            {
                user.AccessFailedCount++;
                var ex = new SignInException
                {
                    IsWrongCode = true,
                };

                if (user.AccessFailedCount >= _authConfig.MaxSignInTryCount)
                {
                    user.VerificationCode = null;
                    user.AccessFailedCount = 0;

                    ex.NeedToRequestVerifyCode = true;
                } else
                {                    
                    ex.AttemptsLeft = _authConfig.MaxSignInTryCount - user.AccessFailedCount;
                }
                await _context.SaveChangesAsync();

                throw ex;
            }
        }

        public async Task SendCodeByEmailAsync(string email, string code)
        {
            await _messengerService.SendMessageAsync(
                MessageType.Email, 
                email, "Код подтверждения", 
                $"Код: {code}");
        }

        public async Task<string> UpdateVerificationCodeAsync(User user)
        {
            user.AccessFailedCount = 0;
            var code = GenerateVerificationCode();
            user.VerificationCode = code;

            await _context.SaveChangesAsync();

            return code;
        }

        private static string GenerateVerificationCode()
        {
            return new Random().Next(1000, 9999).ToString();
        }
    }
}
