using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using queryHelp.DTOs;
using queryHelp.Interfaces;

namespace queryHelp.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiService _ai;

        public AiController(IAiService ai)
        {
            _ai = ai;
        }

        [HttpPost("suggest")]
        public async Task<IActionResult> Suggest([FromBody] AiSuggestRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.NlText)) return BadRequest("nlText is required.");

            var res = await _ai.GenerateSqlAsync(req.DataSourceId, req.NlText, ct);

            if (!res.IsSafe)
            {
                return BadRequest(new
                {
                    ok = false,
                    message = "Generated SQL marked unsafe.",
                    reason = res.SafetyReason,
                    sql = res.Sql,
                    explain = res.Explain
                });
            }

            return Ok(new
            {
                ok = true,
                sql = res.Sql,
                explain = res.Explain,
                parameters = res.Parameters
            });
        }
    }
}