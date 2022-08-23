using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController:BaseApiController
    {
        private readonly DataContext _context;
        public ITokenService _tokenService { get; set; }
        public AccountController(DataContext context,ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
            
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if(await USerExists(registerDTO.Username))return BadRequest("Username is taken");
            using var hmac=new HMACSHA512();

            var user=new AppUser{
                UserName=registerDTO.Username.ToLower(),
                PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt=hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }

        private async Task<bool> USerExists(string username)
        {
            return await _context.Users.AnyAsync(x=>x.UserName==username.ToLower());
        }

          [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user= await _context.Users.SingleOrDefaultAsync(x=>x.UserName==loginDto.Username);
            if(user==null )return Unauthorized("Invalid User");

             using var hmac=new HMACSHA512(user.PasswordSalt);
             var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
             for(int i=0;i<computedHash.Length;i++)
             {
                if(computedHash[i]!=user.PasswordHash[i])return Unauthorized("Invalid Password");
             }
               return new UserDto
            {
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }
    }
}