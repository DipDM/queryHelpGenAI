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

namespace queryHelp.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QueryRunsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public QueryRunsController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var run = await _db.QueryRuns.FindAsync(id);
            if (run == null) return NotFound();
            return Ok(_mapper.Map<QueryRunReadDto>(run));
        }


        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> List([FromQuery] PaginationParams p)
        {
            var q = _db.QueryRuns.AsNoTracking().OrderByDescending(r => r.CreatedAt);
            var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync();
            return Ok(items.Select(r => _mapper.Map<QueryRunReadDto>(r)));
        }
    }
}