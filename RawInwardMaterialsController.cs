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

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawInwardMaterialsController : ControllerBase
    {
        private readonly Lg202324Context _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RawInwardMaterialsController> _logger;

        public RawInwardMaterialsController(Lg202324Context context, IMapper mapper, ILogger<RawInwardMaterialsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/RawInwardMaterials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RawInwardMaterialReadOnlyDto>>> GetRawInwardMaterials()
        {
            try
            {
                if (_context.RawInwardMaterials == null)
                {
                    _logger.LogWarning("Attempted to retrieve raw inward materials, but the entity set was null.");
                    return NotFound();
                }

                var materials = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs) // Include child entities
                    .OrderByDescending(m => m.InMatId)
                    .ToListAsync();

                var materialsDto = _mapper.Map<IEnumerable<RawInwardMaterialReadOnlyDto>>(materials);

                _logger.LogInformation("Retrieved all raw inward materials with their subs.");
                return Ok(materialsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving raw inward materials.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // GET: api/RawInwardMaterials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RawInwardMaterialReadOnlyDto>> GetRawInwardMaterial(int id)
        {
            try
            {
                var material = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs) // Include child entities
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (material == null)
                {
                    _logger.LogWarning($"Raw inward material with ID {id} was not found.");
                    return NotFound();
                }

                var materialDto = _mapper.Map<RawInwardMaterialReadOnlyDto>(material);

                _logger.LogInformation($"Retrieved raw inward material with ID {id} and its subs.");
                return Ok(materialDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving raw inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        // PUT: api/RawInwardMaterials/5
        [HttpPut("{id}")]
       
        public async Task<IActionResult> PutRawInwardMaterial(int id, RawInwardMaterialCreateOnlyDto materialDto)
        {
            if (id != materialDto.InMatId)
            {
                _logger.LogWarning("PUT request failed due to mismatched IDs.");
                return BadRequest();
            }

            try
            {
                // Load the existing entity, including child entities
                var existingMaterial = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs)
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (existingMaterial == null)
                {
                    _logger.LogWarning($"Raw inward material with ID {id} does not exist.");
                    return NotFound();
                }

                // Map the incoming data to the existing entity
                _mapper.Map(materialDto, existingMaterial);

                // Handle child entities (e.g., update or remove existing, add new)
                if (materialDto.RawInwardMaterialSubs != null)
                {
                    // Remove subs that are not in the incoming DTO
                    var subsToRemove = existingMaterial.RawInwardMaterialSubs
                        .Where(sub => !materialDto.RawInwardMaterialSubs.Any(dtoSub => dtoSub.InMatSubId == sub.InMatSubId))
                        .ToList();
                    _context.RawInwardMaterialSubs.RemoveRange(subsToRemove);

                    // Update or add subs
                    foreach (var dtoSub in materialDto.RawInwardMaterialSubs)
                    {
                        var existingSub = existingMaterial.RawInwardMaterialSubs
                            .FirstOrDefault(sub => sub.InMatSubId == dtoSub.InMatSubId);

                        if (existingSub != null)
                        {
                            // Update existing sub
                            _mapper.Map(dtoSub, existingSub);
                        }
                        else
                        {
                            // Add new sub
                            var newSub = _mapper.Map<RawInwardMaterialSub>(dtoSub);
                            existingMaterial.RawInwardMaterialSubs.Add(newSub);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated raw inward material with ID {id}.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RawInwardMaterialExists(id))
                {
                    _logger.LogWarning($"Raw inward material with ID {id} does not exist.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, $"Concurrency error occurred while updating raw inward material with ID {id}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating raw inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }


        // POST: api/RawInwardMaterials
        [HttpPost]
        public async Task<ActionResult<RawInwardMaterialReadOnlyDto>> PostRawInwardMaterial(RawInwardMaterialCreateOnlyDto materialDto)
        {
            if (_context.RawInwardMaterials == null)
            {
                _logger.LogWarning("Attempted to create a raw inward material but the entity set was null.");
                return Problem("Entity set 'Lg202324Context.RawInwardMaterials' is null.");
            }

            try
            {
                // Map the incoming DTO to the entity
                var material = _mapper.Map<RawInwardMaterial>(materialDto);

                // Add the material to the context and save changes
                _context.RawInwardMaterials.Add(material);
                await _context.SaveChangesAsync();

                // Retrieve the created material along with its child entities
                var createdMaterial = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs) // Including related child entities
                    .FirstOrDefaultAsync(m => m.InMatId == material.InMatId);

                // If the material is not found after saving, log a warning and return not found
                if (createdMaterial == null)
                {
                    _logger.LogWarning("Failed to retrieve the created raw inward material.");
                    return NotFound("The newly created raw inward material could not be found.");
                }

                // Map the created material entity back to a read-only DTO for returning in the response
                var createdMaterialDto = _mapper.Map<RawInwardMaterialReadOnlyDto>(createdMaterial);

                _logger.LogInformation($"Created a new raw inward material with ID {material.InMatId}.");

                // Return the created material along with a URI to access it via GET
                return CreatedAtAction(nameof(GetRawInwardMaterial), new { id = material.InMatId }, createdMaterialDto);
            }
            catch (Exception ex)
            {
                // Log and handle any unexpected errors
                _logger.LogError(ex, "An error occurred while creating a raw inward material.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }


        // DELETE: api/RawInwardMaterials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRawInwardMaterial(int id)
        {
            try
            {
                var material = await _context.RawInwardMaterials
                    .Include(m => m.RawInwardMaterialSubs) // Include child entities
                    .FirstOrDefaultAsync(m => m.InMatId == id);

                if (material == null)
                {
                    _logger.LogWarning($"Raw inward material with ID {id} was not found.");
                    return NotFound();
                }

                _context.RawInwardMaterials.Remove(material);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted raw inward material with ID {id}.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting raw inward material with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }

        private bool RawInwardMaterialExists(int id)
        {
            return (_context.RawInwardMaterials?.Any(m => m.InMatId == id)).GetValueOrDefault();
        }
    }
}
