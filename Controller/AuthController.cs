using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using queryHelp.Data;
using queryHelp.DTOs;
using queryHelp.Interfaces;
using queryHelp.Models;

namespace queryHelp.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
                return Conflict("Username or email already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "user"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto req)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == req.Username);
            if (user == null) return Unauthorized("Invalid credentials.");

            var valid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
            if (!valid) return Unauthorized("Invalid credentials.");

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }
    }
}