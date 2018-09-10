using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Membership.Models;
using Membership.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Membership.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IOptions<IdentitySettings> _identitySettings;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IOptions<IdentitySettings> identitySettings)
        {
            _repository = repository;
            _identitySettings = identitySettings;

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<UserInfoEntity, UserInfoDto>();
                cfg.CreateMap<UserInfoDto, UserInfoEntity>();
            });
            _mapper = config.CreateMapper();
        }

        public byte[] Sha256ComputeHash(string password, byte[] passwordSalt)
        {
            byte[] passwordHash;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(passwordSalt))
            {
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }

            return passwordHash;
        }

        public byte[] Sha256GenerateRandomSalt()
        {
            byte[] passwordSalt;
            using (var hmac = new System.Security.Cryptography.HMACSHA256())
            {
                passwordSalt = hmac.Key;
            }

            return passwordSalt;
        }


        public bool ValidateUser(string username, string password, UserInfoEntity userInfoEntity)
        {
            // TODO: get salt, generate passwordhash, compare
            var passwordSalt = userInfoEntity.PasswordSalt;
            var expectedPasswordHash = Sha256ComputeHash(password, passwordSalt);
            var actualPasswordHash = userInfoEntity.PasswordHash;
            var result = CheckByteEquals(expectedPasswordHash, actualPasswordHash);
            return result;
        }

        private bool CheckByteEquals(byte[] expectedPasswordHash, byte[] actualPasswordHash)
        {
            if (expectedPasswordHash.Length != actualPasswordHash.Length)
                return false;
            for (int i = 0; i < expectedPasswordHash.Length; i++)
            {
                if (expectedPasswordHash[i] != actualPasswordHash[i])
                    return false;
            }

            return true;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var exsitUserInfoEntity = await _repository.GetUserByUsernameAsync(username);
            var validateResult = ValidateUser(username, password, exsitUserInfoEntity);
            return validateResult;
        }

        public string GenerateToken(string username)
        {
            var symmetricKey = Convert.FromBase64String(_identitySettings.Value.Secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Role, "Administrator")
                }),
                Audience = "A",
                Issuer = "Membership",
                Expires = DateTime.Now.AddSeconds(_identitySettings.Value.TokenLifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256)
            };
            try
            {
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);
                return tokenString;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<UserInfoDto> GetUserByUsernameAsync(string username)
        {
            var userInfo = await _repository.GetUserByUsernameAsync(username);
            var userDto = _mapper.Map<UserInfoDto>(userInfo);
            return userDto;
        }

        public async Task<bool> CheckUserExsits(string username)
        {
            // TODO: check if exsit
            var exsitUserInfo = await _repository.GetUserByUsernameAsync(username);
            // TODO: throw what exception
            return exsitUserInfo != null;
        }

        public async Task<UserInfoDto> CreateUserAsync(UserInfoDto userInfoDto)
        {
            // TODO: how to deal with Id

            // TODO: hash password
            var passwordSalt = Sha256GenerateRandomSalt();
            var passwordHash = Sha256ComputeHash(userInfoDto.Password, passwordSalt);

            // TODO: map UserInfoDto into UserInfoEntity
            
            var userInfoEntity = _mapper.Map<UserInfoEntity>(userInfoDto);
            userInfoEntity.PasswordSalt = passwordSalt;
            userInfoEntity.PasswordHash = passwordHash;

            // TODO: save hashed password into database
            var result = await _repository.CreateUserAsync(userInfoEntity);
            // TODO: throw what exception
            //if (result != 1) return;
            var newUserInfoEntity = await _repository.GetUserByUsernameAsync(userInfoEntity.Username);
            // TODO: map UserInfoEntity into UserInfoDto
            var newUserInfoDto = _mapper.Map<UserInfoDto>(newUserInfoEntity);
            newUserInfoDto.Password = null;
            return newUserInfoDto;
        }

        public async void UpdateUserPasswordAsync(UserInfoDto userInfoDto)
        {
            var passwordSalt = Sha256GenerateRandomSalt();
            var passwordHash = Sha256ComputeHash(userInfoDto.Password, passwordSalt);

            var userInfoEntity = _mapper.Map<UserInfoEntity>(userInfoDto);
            userInfoEntity.PasswordSalt = passwordSalt;
            userInfoEntity.PasswordHash = passwordHash;
            var result = await _repository.UpdatePasswordAsync(userInfoEntity);
            // TODO: throw what exception
            //if (result != 1) return;
        }

        public async void ActivateAsync(UserInfoDto userInfoDto, string clientId)
        {
            var userInfoEntity = _mapper.Map<UserInfoEntity>(userInfoDto);
            SetClientActive(userInfoEntity, clientId, true);
            var result = await _repository.UpdateAsync(userInfoEntity);
            // TODO: throw what exception
            //if (result != 1) return;
        }

        public async void DeActivateAsync(UserInfoDto userInfoDto, string clientId)
        {
            var userInfoEntity = _mapper.Map<UserInfoEntity>(userInfoDto);
            SetClientActive(userInfoEntity, clientId, false);
            var result = await _repository.UpdateAsync(userInfoEntity);
            // TODO: throw what exception
            //if (result != 1) return;
        }

        private void SetClientActive(UserInfoEntity userInfoDto, string clientId, bool active)
        {
            switch (clientId)
            {
                case "A":
                    userInfoDto.ActivateStatusA = active;
                    break;
                case "B":
                    userInfoDto.ActivateStatusB = active;
                    break;
                case "C":
                    userInfoDto.ActivateStatusC = active;
                    break;
                default:
                    break;
            }
        }
    }
}
