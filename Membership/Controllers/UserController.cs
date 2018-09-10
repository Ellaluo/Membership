using System.Linq;
using System.Threading.Tasks;
using Membership.Models;
using Membership.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Membership.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        /// <summary>
        ///     Authenticate User Endpoint
        /// </summary>
        [AllowAnonymous]
        [Route("Authenticate", Name = "Authenticate_User")]
        [HttpPost, HttpOptions]
        public async Task<IActionResult> Authenticate([FromBody] UserInfoDto userInfoDto)
        {
            if (string.IsNullOrEmpty(userInfoDto.Username) || string.IsNullOrEmpty(userInfoDto.Password))
                return BadRequest("Invalid username or password");
            if (!await _service.CheckUserExsits(userInfoDto.Username))
            {
                return Unauthorized();
            }
            // TODO: check username and password valid
            var validateResult = await _service.ValidateUserAsync(userInfoDto.Username, userInfoDto.Password);
            // TODO: generate JWT token
            if (!validateResult)
            {
                return Unauthorized();
            }
            var tokenString = _service.GenerateToken(userInfoDto.Username);
            return Ok(tokenString);
        }

        /// <summary>
        ///     Create User Endpoint
        /// </summary>
        /// <param name="userInfoDto"></param>
        /// <returns>Get_User Route</returns>
        /// <response code = "201">Created</response>
        [AllowAnonymous]
        [Route("", Name = "Create_New_User")]
        [HttpPost, HttpOptions]
        public async Task<IActionResult> CreateUser([FromBody] UserInfoDto userInfoDto)
        {
            if (await _service.CheckUserExsits(userInfoDto.Username))
            {
                return Conflict();
            }

            // TODO: Id provided or not
            var result = await _service.CreateUserAsync(userInfoDto);
            // TODO: return userInfoDto or return route
            return Ok(result);
        }

        /// <summary>
        ///     Update Password Endpoint
        /// </summary>
        /// <param name="userInfoDto"></param>
        /// < returns > Get_User Route</returns>
        /// <response code = "202" > Updated </response >
        [Route("", Name = "Update_User_Password")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserPassword([FromBody] UserInfoDto userInfoDto)
        {
            var username = User.Identity.Name;
            var existingUserInfoDto = await _service.GetUserByUsernameAsync(username);
            existingUserInfoDto.Password = userInfoDto.Password;

            _service.UpdateUserPasswordAsync(existingUserInfoDto);
            return Accepted();
        }

        [Route("activate", Name = "Activate_User")]
        [HttpPut, HttpOptions]
        public async Task<IActionResult> Activate([FromBody] UserInfoDto userInfoDto)
        {

            // TODO: check if admin roles
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            var username = userInfoDto.Username;
            var existingUserInfoDto = await _service.GetUserByUsernameAsync(username);
            if (existingUserInfoDto == null)
            {
                return NotFound();
            }

            var client = User.Claims.First(x => x.Type == "aud").Value;

            // TODO: ativate on its own app
            _service.ActivateAsync(existingUserInfoDto, client);
            return Ok();
        }

        [Route("deactivate", Name = "Deactivate_User")]
        [HttpPut, HttpOptions]
        public async Task<IActionResult> DeActivate([FromBody] UserInfoDto userInfoDto)
        {
            // TODO: check if admin roles
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            if (!await _service.CheckUserExsits(userInfoDto.Username))
            {
                return NotFound();
            }

            var username = userInfoDto.Username;
            var existingUserInfoDto = await _service.GetUserByUsernameAsync(username);
            var client = User.Claims.First(x => x.Type == "aud").Value;

            // TODO: ativate on its own app
            _service.DeActivateAsync(existingUserInfoDto, client);
            return Ok();
        }
    }
}