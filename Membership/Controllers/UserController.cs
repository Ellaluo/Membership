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
        /// <param name="userInfoDto"></param>
        /// <response code = "200"> Ok </response>
        /// <response code = "400"> BadRequest </response>
        /// <response code = "401"> Unauthorized </response>
        [AllowAnonymous]
        [Route("Authenticate", Name = "Authenticate_User")]
        [HttpPost, HttpOptions]
        public async Task<IActionResult> Authenticate([FromBody] UserInfoDto userInfoDto)
        {
            if (!userInfoDto.Client.Equals("A") && !userInfoDto.Client.Equals("B") && !userInfoDto.Client.Equals("C"))
            {
                return BadRequest("Invalid client");
            }

            if (string.IsNullOrEmpty(userInfoDto.Username) || string.IsNullOrEmpty(userInfoDto.Password))
                return BadRequest("Invalid username or password");
            
            var existingUserInfoDto = await _service.GetUserByUsernameAsync(userInfoDto.Username);
            if (existingUserInfoDto == null)
            {
                return Unauthorized();
            }
            
            var validateResult = await _service.ValidateUserAsync(userInfoDto.Username, userInfoDto.Password);
            
            if (!validateResult)
            {
                return Unauthorized();
            }
            
            var tokenString = _service.GenerateToken(userInfoDto.Username, userInfoDto.Client);
            return Ok(tokenString);
        }

        /// <summary>
        ///     Create User Endpoint
        /// </summary>
        /// <param name="userInfoDto"></param>
        /// <response code = "201"> Created </response>
        /// <response code = "400"> BadRequest </response>
        /// <response code = "409"> Conflict </response>
        [AllowAnonymous]
        [Route("", Name = "Create_New_User")]
        [HttpPost, HttpOptions]
        public async Task<IActionResult> CreateUser([FromBody] UserInfoDto userInfoDto)
        {
            var existingUserInfoDto = await _service.GetUserByUsernameAsync(userInfoDto.Username);
            if (existingUserInfoDto != null)
            {
                return Conflict();
            }
            
            var result = await _service.CreateUserAsync(userInfoDto);
            if (result == null) return BadRequest(); 
            return Ok(result);
        }

        /// <summary>
        ///     Update Password Endpoint
        /// </summary>
        /// <param name="userInfoDto"></param>
        /// <response code = "202" > Updated </response >
        /// <response code = "400" > BadRequest </response >
        [Route("Password", Name = "Update_User_Password")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserPassword([FromBody] UserInfoDto userInfoDto)
        {
            var username = User.Identity.Name;
         
            var existingUserInfoDto = await _service.GetUserByUsernameAsync(username);
            existingUserInfoDto.Password = userInfoDto.Password;
            var result = await _service.UpdateUserPasswordAsync(existingUserInfoDto);
            if (result != 1) return BadRequest();
            return Accepted();
        }

        /// <summary>
        ///     Activate Endpoint
        /// </summary>
        /// <param name="userInfoDto"></param>
        /// <response code = "200" > Ok </response >
        /// <response code = "400" > BadRequest </response >
        /// <response code = "403" > Forbid </response >
        /// <response code = "404" > NotFound </response >
        [Route("activate", Name = "Activate_User")]
        [HttpPut, HttpOptions]
        public async Task<IActionResult> Activate([FromBody] UserInfoDto userInfoDto)
        {
            
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
            
            var result = await _service.ActivateAsync(existingUserInfoDto, client);
            if (result != 1) return BadRequest();
            return Ok();
        }

        /// <summary>
        ///     Dectivate Endpoint
        /// </summary>
        /// <param name="userInfoDto"></param>
        /// <response code = "200" > Ok </response >
        /// <response code = "403" > Forbid </response >
        /// <response code = "404" > NotFound </response >
        [Route("deactivate", Name = "Deactivate_User")]
        [HttpPut, HttpOptions]
        public async Task<IActionResult> DeActivate([FromBody] UserInfoDto userInfoDto)
        {
            if (!User.IsInRole("Administrator"))
            {
                return Forbid();
            }
            
            var existingUserInfoDto = await _service.GetUserByUsernameAsync(userInfoDto.Username);
            if (existingUserInfoDto == null)
            {
                return NotFound();
            }
            
            var client = User.Claims.First(x => x.Type == "aud").Value;
            
            var result = await _service.DeActivateAsync(existingUserInfoDto, client);
            if (result != 1) return BadRequest();
            return Ok();
        }
    }
}