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
    public class SavedQueriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public SavedQueriesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationParams p)
        {
            var ownerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var q = _db.SavedQueries.Where(s => s.OwnerId == ownerId).AsNoTracking().OrderBy(s => s.Id);
            var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync();
            return Ok(items.Select(s => _mapper.Map<SavedQueryReadDto>(s)));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ownerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var s = await _db.SavedQueries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId);
            if (s == null) return NotFound();
            return Ok(_mapper.Map<SavedQueryReadDto>(s));
        }


        [HttpPost]
        public async Task<IActionResult> Create(SavedQueryCreateDto dto)
        {
            var ownerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var s = _mapper.Map<SavedQuery>(dto);
            s.OwnerId = ownerId;
            _db.SavedQueries.Add(s);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = s.Id }, _mapper.Map<SavedQueryReadDto>(s));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SavedQueryUpdateDto dto)
        {
            var ownerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var s = await _db.SavedQueries.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId);
            if (s == null) return NotFound();
            _mapper.Map(dto, s);
            s.UpdatedAt = System.DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ownerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var s = await _db.SavedQueries.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId);
            if (s == null) return NotFound();
            _db.SavedQueries.Remove(s);
            await _db.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost("{id}/run")]
        public async Task<IActionResult> Run(int id, [FromBody] QueryRunRequestDto dto)
        {
            // enqueue a run for background worker or run synchronously (demo)
            var ownerId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var s = await _db.SavedQueries.FindAsync(id);
            if (s == null || s.OwnerId != ownerId) return NotFound();


            var run = new QueryRun
            {
                SavedQueryId = s.Id,
                UserId = ownerId,
                ParametersJson = dto.ParametersJson,
                StartedAt = System.DateTime.UtcNow,
                Status = "Pending"
            };
            _db.QueryRuns.Add(run);
            await _db.SaveChangesAsync();


            // For MVP: worker would pick this up and execute. Return run id.
            return Accepted(new { runId = run.Id });
        }
    }
}