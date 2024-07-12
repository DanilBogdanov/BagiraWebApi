using BagiraWebApi.Dtos.Auth;
using BagiraWebApi.Exceptions.Auth;
using BagiraWebApi.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BagiraWebApi.Controllers.api.Auth.v1
{
    [Route("api/auth/v1")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signin/anonimous")]
        public async Task<IActionResult> SignInAnonimousAsync(SignInBaseRequest signInRequest)
        {
            try
            {
                var tokens = await _authService.SignInAnonimous(signInRequest);

                return Ok(tokens);
            }
            catch (ClientValidationException)
            {
                return Forbid("Client is invalid");
            }
        }

        [Authorize]
        [HttpPost("signin/email")]
        public async Task<IActionResult> SignInEmailAsync(SignInRequest signInRequestDTO)
        {
            try
            {
                var tokens = await _authService.SignInEmail(signInRequestDTO);

                return Ok(tokens);
            }
            catch (ClientValidationException)
            {
                return Forbid("Client is invalid");
            }
            catch (UserNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (SignInException ex)
            {
                return BadRequest(new
                {
                    ex.NeedToRequestVerifyCode,
                    ex.AttemptsLeft,
                    ex.IsWrongCode
                });
            }
        }

        [Authorize]
        [HttpPost("verify/email")]
        public async Task<IActionResult> VerifyEmailAsync(string email)
        {
            await _authService.SendVerificationCodeByEmailAsync(email);

            return Ok();
        }

        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOutAsync()
        {
            var claimsPrincipal = HttpContext.User;

            try
            {
                await _authService.SignOut(claimsPrincipal);
                return Ok();
            }
            catch (SessionNotFoundException)
            {
                return Forbid("Session not found");
            }
            catch (SessionValidationException)
            {
                return Forbid("Invalid session token");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync()
        {
            var claimsPrincipal = HttpContext.User;

            try
            {
                var tokens = await _authService.RefreshAsync(claimsPrincipal);
                return Ok(tokens);
            }
            catch (SessionNotFoundException)
            {
                return Forbid("Session not found");
            }
            catch (SessionValidationException)
            {
                return Forbid("Invalid session token");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
