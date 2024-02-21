using BagiraWebApi.Services.Auth;
using BagiraWebApi.Services.Auth.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Auth.v1
{
    [Route("api/auth/v1")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInForm signInForm)
        {
            var res = await _authService.SignIn(signInForm);
            Response.Cookies.Append("refresh", res.RefreshToken, new CookieOptions
            {
                Path = "/api/auth",
                HttpOnly = true,
                Secure = true,
                IsEssential = true
            });

            return Ok(res);
        }


        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(SignUpUserForm userForm)
        {
            var createdId = await _authService.SignUpAsync(userForm);
            return Ok(createdId);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {

            return Ok("test");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync()
        {
            if (Request.Cookies.TryGetValue("refresh", out var refreshToken))
            {
                var tokens = await _authService.RefreshToken(refreshToken);
                Response.Cookies.Append("refresh", tokens.RefreshToken, new CookieOptions
                {
                    Path = "/api/auth",
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true
                });

                return Ok(tokens);
            }

            return Unauthorized();
        }
    }
}
