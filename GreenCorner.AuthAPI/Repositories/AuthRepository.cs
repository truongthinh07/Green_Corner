﻿using GreenCorner.AuthAPI.Data;
using GreenCorner.AuthAPI.Models;
using GreenCorner.AuthAPI.Models.DTO;
using GreenCorner.AuthAPI.Repositories.Interface;
using GreenCorner.AuthAPI.Services.Interface;
using Microsoft.AspNetCore.Identity;

namespace GreenCorner.AuthAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthDbContext _dbcontext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthRepository(AuthDbContext dbcontext, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _dbcontext = dbcontext;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = _dbcontext.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDto)
        {
            var user = _dbcontext.Users.FirstOrDefault(u => u.Email == loginRequestDto.Email);

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (user == null || !isValid)
            {
                return new LoginResponseDTO()
                {
                    User = null,
                    Token = ""
                };
            }

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            UserDTO userDto = new()
            {
                ID = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                Avatar = user.Avatar,
                PhoneNumber = user.PhoneNumber
            };
            LoginResponseDTO loginResponseDto = new LoginResponseDTO()
            {
                User = userDto,
                Token = token
            };
            return loginResponseDto;
        }

        public async Task<string> Register(RegisterationRequestDTO registrationRequestDto)
        {
            User user = new User()
            {
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                FullName = registrationRequestDto.FullName,
                Address = registrationRequestDto.Address,
                Avatar =  registrationRequestDto.Avatar,
                PhoneNumber = registrationRequestDto.PhoneNumber
            };

            try 
            { 
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _dbcontext.Users.Where(u => u.Email == registrationRequestDto.Email).FirstOrDefault();

                    UserDTO userDto = new()
                    {
                        ID = userToReturn.Id,
                        Email = userToReturn.Email,
                        FullName = userToReturn.FullName,
                        Address = userToReturn.Address,
                        Avatar = userToReturn.Avatar
                    };
                    return "";
                }
                else 
                { 
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Error creating user: " + ex.Message);
            }
            return "Error Encountered";
        }
    }
}
