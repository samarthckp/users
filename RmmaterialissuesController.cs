 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using ERP_API.Data;
using ERP_API.Moduls;
using Azure.Core;
using Humanizer;
using NuGet.Packaging.Signing;

namespace ERP_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RmmaterialissuesController : ControllerBase
    {
        private readonly Lg202324Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RmmaterialissuesController> _logger;

        public RmmaterialissuesController(Lg202324Context context, IMapper mapper, ILogger<RmmaterialissuesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Rmmaterialissues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RmmaterialissueReadOnly>>> GetRmmaterialissue()
        {
            if (_context.Rmmaterialissues == null)
            {
                _logger.LogWarning("Attempted to retrieve material issues but the entity set was null.");
                return NotFound();
            }

            var issues = await _context.Rmmaterialissues
                .Include(e => e.RmmaterialissueSubs)
                    .OrderByDescending(e => e.matIssueId)
                    .ToListAsync();
            var issuesDto = _mapper.Map<IEnumerable<RmmaterialissueReadOnly>>(issues);

            _logger.LogInformation("Retrieved all material issues.");
            return Ok(issuesDto);
        }

        // GET: api/Rmmaterialissues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RmmaterialissueReadOnly>> GetRmmaterialissue(int? id)
        {
            if (_context.Rmmaterialissues == null)
            {
                _logger.LogWarning("Attempted to retrieve a material issue but the entity set was null.");
                return NotFound();
            }

            var rmmaterialissue = await _context.Rmmaterialissues.
                Include(e => e.RmmaterialissueSubs)
                    .FirstOrDefaultAsync(e => e.matIssueId == id); ;

            if (rmmaterialissue == null)
            {
                _logger.LogWarning($"Material issue with ID {id} was not found.");
                return NotFound();
            }

            var issueDto = _mapper.Map<RmmaterialissueReadOnly>(rmmaterialissue);

            _logger.LogInformation($"Retrieved material issue with ID {id}.");
            return Ok(issueDto);
        }

        // PUT: api/Rmmaterialissues/5
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRmmaterialissue(int? id, RmmaterialissueCreateOnly materialDto)
        {
            if (id != materialDto.matIssueId)
            {
                _logger.LogWarning("PUT request failed due to mismatched IDs.");
                return BadRequest();
            }

            try
            {
                // Load the existing entity, including child entities
                var existingMaterial = await _context.Rmmaterialissues
                    .Include(m => m.RmmaterialissueSubs)
                    .FirstOrDefaultAsync(m => m.matIssueId == id);

                if (existingMaterial == null)
                {
                    _logger.LogWarning($"Raw inward material with ID {id} does not exist.");
                    return NotFound();
                }

                // Map the incoming data to the existing entity
                _mapper.Map(materialDto, existingMaterial);

                // Handle child entities (e.g., update or remove existing, add new)
                if (materialDto.RmmaterialissueSubs != null)
                {
                    // Remove subs that are not in the incoming DTO
                    var subsToRemove = existingMaterial.RmmaterialissueSubs
                        .Where(sub => !materialDto.RmmaterialissueSubs.Any(dtoSub => dtoSub.matIssueSubId == sub.matIssueSubId))
                        .ToList();
                    _context.RmmaterialissueSubs.RemoveRange(subsToRemove);

                    // Update or add subs
                    foreach (var dtoSub in materialDto.RmmaterialissueSubs)
                    {
                        var existingSub = existingMaterial.RmmaterialissueSubs
                            .FirstOrDefault(sub => sub.matIssueSubId == dtoSub.matIssueSubId);

                        if (existingSub != null)
                        {
                            // Update existing sub
                            _mapper.Map(dtoSub, existingSub);
                        }
                        else
                        {
                            // Add new sub
                            var newSub = _mapper.Map<RmmaterialissueSub>(dtoSub);
                            existingMaterial.RmmaterialissueSubs.Add(newSub);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated  material issue with ID {id}.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RmmaterialissueExists(id))
                {
                    _logger.LogWarning($"Material issue with ID {id} does not exist.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, $"Concurrency error occurred while updating raw material issue with ID {id}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating raw material issue with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }
        // POST: api/Rmmaterialissues
        [HttpPost]
        public async Task<ActionResult<RmmaterialissueReadOnly>> PostRmmaterialissue(RmmaterialissueCreateOnly rmmaterialissueDto)
        {
            if (_context.Rmmaterialissues == null)
            {
                _logger.LogWarning("Attempted to create a material issue but the entity set was null.");
                return Problem("Entity set 'Lg202324Context.Rmmaterialissue' is null.");
            }

            var materialIssue = _mapper.Map<Rmmaterialissue>(rmmaterialissueDto);
            _context.Rmmaterialissues.Add(materialIssue);
            await _context.SaveChangesAsync();

            var createdMaterialIssue = await _context.Rmmaterialissues
                  .Include(e => e.RmmaterialissueSubs)
                  .FirstOrDefaultAsync(e => e.matIssueId == materialIssue.matIssueId);

            var createdIssueDto = _mapper.Map<RmmaterialissueReadOnly>(createdMaterialIssue);

            _logger.LogInformation($"Created a new material issue with ID {materialIssue.matIssueId}.");
            return CreatedAtAction(nameof(GetRmmaterialissue), new { id = materialIssue.matIssueId }, createdIssueDto);
        }

        // DELETE: api/Rmmaterialissues/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRmmaterialissue(int? id)
        {
            if (_context.Rmmaterialissues == null)
            {
                _logger.LogWarning("Attempted to delete a material issue but the entity set was null.");
                return NotFound();
            }

            var rmmaterialissue = await _context.Rmmaterialissues.
                Include(e => e.RmmaterialissueSubs)
                  .FirstOrDefaultAsync(e => e.matIssueId == id); 
            if (rmmaterialissue == null)
            {
                _logger.LogWarning($"Material issue with ID {id} was not found.");
                return NotFound();
            }

            _context.Rmmaterialissues.Remove(rmmaterialissue);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted material issue with ID {id}.");
            return NoContent();
        }

        private bool RmmaterialissueExists(int? id)
        {
            return (_context.Rmmaterialissues?.Any(e => e.matIssueId == id)).GetValueOrDefault();
        }
    }
}
