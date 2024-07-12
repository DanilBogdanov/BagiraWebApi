using BagiraWebApi.Configs.Auth;
using BagiraWebApi.Dtos.Auth;
using BagiraWebApi.Exceptions.Auth;
using BagiraWebApi.Models.Auth;
using BagiraWebApi.Services.Auth.Services;
using BagiraWebApi.Services.Messengers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace BagiraWebApi.Services.Auth
{
    public class AuthService
    {
        private readonly AuthConfig _authConfig;
        private readonly UserService _userService;
        private readonly VerifyService _verifyService;
        private readonly SessionService _sessionService;
        private readonly TokenService _tokenService;

        public AuthService(IOptions<AuthConfig> options, ApplicationContext context, MessengerService messengerService)
        {
            _authConfig = options.Value;
            _userService = new UserService(context);
            _verifyService = new VerifyService(context, _authConfig, messengerService);
            _sessionService = new SessionService(context, _authConfig);
            _tokenService = new TokenService(_authConfig);
        }

        public async Task<TokensDto> SignInAnonimous(SignInBaseRequest signInRequest)
        {
            var tokens = await SignIn(null, signInRequest);

            return tokens;
        }

        public async Task<TokensDto> SignInEmail(SignInRequest signInRequestDTO)
        {
            var user = await _userService.GetUserByEmailAsync(signInRequestDTO.Login)
                ?? throw new UserNotFoundException();
            await _verifyService.CheckCode(user, signInRequestDTO.Code);

            var tokens = await SignIn(user, signInRequestDTO);

            return tokens;
        }        

        public async Task<TokensDto> RefreshAsync(ClaimsPrincipal claimsPrincipal)
        {
            var session = await GetValidSessionAsync(claimsPrincipal);
            await _sessionService.RefreshSessionAsync(session);
            var tokens = _tokenService.GetTokens(session);

            return tokens;
        }

        public async Task SignOut(ClaimsPrincipal claimsPrincipal)
        {
            var session = await GetValidSessionAsync(claimsPrincipal);
            await _sessionService.RemoveSessionAsync(session);
        }

        public async Task SendVerificationCodeByEmailAsync(string login)
        {
            var user = await _userService.GetUserByEmailAsync(login)
                ?? await _userService.CreateUserByEmailAsync(login);
            var code = await _verifyService.UpdateVerificationCodeAsync(user);
            await _verifyService.SendCodeByEmailAsync(login, code);
        }

        private async Task<TokensDto> SignIn(User? user, SignInBaseRequest signInRequest)
        {
            _verifyService.CheckClientIsValid(signInRequest.ClientId, signInRequest.ClientSecret);
            
            var session = await _sessionService.CreateSessionAsync(user);
            var tokens = _tokenService.GetTokens(session);

            return tokens;
        }

        private async Task<Session> GetValidSessionAsync(ClaimsPrincipal claimsPrincipal)
        {
            var sessionId = claimsPrincipal.FindFirstValue("sessionId")
                ?? throw new ArgumentException("SessionId is required field");
            var session = await _sessionService.GetSessionAsync(sessionId);
            var sessionToken = claimsPrincipal.FindFirstValue("sessionToken")
                ?? throw new ArgumentException("SessionToken is required field");
            _sessionService.CheckSessionToken(session, sessionToken);
            
            return session;
        }
    }
}
