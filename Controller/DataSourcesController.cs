using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using queryHelp.Data;
using queryHelp.DTOs;
using queryHelp.Interfaces.AiSmartQueryBuilder.Services;
using queryHelp.Models;

namespace queryHelp.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataSourcesController : ControllerBase
    {
        private readonly ISchemaService _schemaService;
         private readonly ApplicationDbContext _db;
        public DataSourcesController(ApplicationDbContext db,ISchemaService schemaService)
        {
            _db = db;
            _schemaService = schemaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DataSource>>> GetAll()
        {
            var list = await _db.DataSources.ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DataSource>> GetById(int id)
        {
            var ds = await _db.DataSources.FindAsync(id);
            if (ds == null) return NotFound();
            return Ok(ds);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DataSourceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var encrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.ConnectionString));

            var ds = new DataSource
            {
                Name = dto.Name,
                EncryptedConnectionString = encrypted,
                IsReadOnly = dto.IsReadOnly,
                CreatedAt = DateTime.UtcNow
            };

            _db.DataSources.Add(ds);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ds.Id }, ds);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DataSourceCreateDto dto)
        {
            var ds = await _db.DataSources.FindAsync(id);
            if (ds == null) return NotFound();

            ds.Name = dto.Name;
            ds.EncryptedConnectionString = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.ConnectionString));
            ds.IsReadOnly = dto.IsReadOnly;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ds = await _db.DataSources.FindAsync(id);
            if (ds == null) return NotFound();

            _db.DataSources.Remove(ds);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("debug/schema/{datasourceId}")]
        [Authorize]
        public async Task<IActionResult> DebugSchema(int datasourceId)
        {
            var summary = await _schemaService.GetSchemaSummaryAsync(datasourceId);
            if (string.IsNullOrWhiteSpace(summary))
                return NotFound("Schema empty - check DataSource exists, connection string, networking and DB permissions.");

            return Ok(summary);
        }

    }
}