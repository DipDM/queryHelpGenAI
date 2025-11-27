using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using queryHelp.Data;
using queryHelp.DTOs;
using queryHelp.Models;

namespace queryHelp.Controller
{
    [ApiController]
    [Route("api/[controller]")]
public class UsersController : ControllerBase
{
private readonly ApplicationDbContext _db;
private readonly IMapper _mapper;
    public UsersController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }


    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Get([FromQuery] PaginationParams p)
    {
        var q = _db.Users.AsNoTracking().OrderBy(u => u.Id);
        var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync();
        return Ok(items.Select(u => _mapper.Map<UserReadDto>(u)));
    }


    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(_mapper.Map<UserReadDto>(user));
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(UserCreateDto dto)
    {
        var user = _mapper.Map<User>(dto);
        // NOTE: Hash password (example uses placeholder)
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        var read = _mapper.Map<UserReadDto>(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, read);
    }


    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UserUpdateDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        _mapper.Map(dto, user);
        user.UpdatedAt = System.DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

}