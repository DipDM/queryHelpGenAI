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
    [Authorize]
    public class QueryTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public QueryTemplatesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationParams p)
        {
            var q = _db.QueryTemplates.AsNoTracking().OrderBy(t => t.Id);
            var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync();
            return Ok(items.Select(t => _mapper.Map<QueryTemplateReadDto>(t)));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _db.QueryTemplates.FindAsync(id);
            if (t == null) return NotFound();
            return Ok(_mapper.Map<QueryTemplateReadDto>(t));
        }


        [HttpPost]
        public async Task<IActionResult> Create(QueryTemplateCreateDto dto)
        {
            var ownerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var t = _mapper.Map<QueryTemplate>(dto);
            t.OwnerId = ownerId;
            _db.QueryTemplates.Add(t);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = t.Id }, _mapper.Map<QueryTemplateReadDto>(t));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, QueryTemplateUpdateDto dto)
        {
            var t = await _db.QueryTemplates.FindAsync(id);
            if (t == null) return NotFound();
            _mapper.Map(dto, t);
            t.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.QueryTemplates.FindAsync(id);
            if (t == null) return NotFound();
            _db.QueryTemplates.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}