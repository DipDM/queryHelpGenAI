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
    [Authorize(Roles = "admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public AuditLogsController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationParams p)
        {
            var q = _db.AuditLogs.AsNoTracking().OrderByDescending(a => a.CreatedAt);
            var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToListAsync();
            return Ok(items);
        }
    }
}