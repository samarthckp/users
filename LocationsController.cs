using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ERP_API.Data;
using ERP_API.Moduls;
using ERP_API.Static;
using Microsoft.Extensions.Logging;

namespace ERP_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly Lg202324Context _context;
        private readonly ILogger<LocationsController> _logger;
        private readonly IMapper _mapper;

        public LocationsController(Lg202324Context context, ILogger<LocationsController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // GET: api/Locations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationReadOnlyDto>>> GetLocations()
        {
            _logger.LogInformation($"Request to {nameof(GetLocations)}");
            try
            {
                var locations = await _context.Locations.ToListAsync();
                var locationDtos = _mapper.Map<IEnumerable<LocationReadOnlyDto>>(locations);
                return Ok(locationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception GET in {nameof(GetLocations)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationReadOnlyDto>> GetLocation(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    _logger.LogWarning($"Location with ID {id} not found in {nameof(GetLocation)}");
                    return NotFound();
                }

                var locationDto = _mapper.Map<LocationReadOnlyDto>(location);
                return Ok(locationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception GET in {nameof(GetLocation)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, LocationCreateDto locationDto)
        {
            if (id != locationDto.LocationId)
            {
                _logger.LogWarning($"Update ID mismatch in {nameof(PutLocation)} - ID: {id}");
                return BadRequest();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                _logger.LogWarning($"Location with ID {id} not found in {nameof(PutLocation)}");
                return NotFound();
            }

            _mapper.Map(locationDto, location);
            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!LocationExists(id))
                {
                    _logger.LogWarning($"Location with ID {id} not found during PUT in {nameof(PutLocation)}");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, $"Concurrency exception PUT in {nameof(PutLocation)}");
                    return StatusCode(500, Messages.Error500Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(PutLocation)}");
                return StatusCode(500, Messages.Error500Message);
            }

            return NoContent();
        }

        // POST: api/Locations
        [HttpPost]
        public async Task<ActionResult<LocationReadOnlyDto>> PostLocation(LocationCreateDto locationDto)
            {
            try
            {
                var location = _mapper.Map<Location>(locationDto);
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                var locationReadOnlyDto = _mapper.Map<LocationReadOnlyDto>(location);
                return CreatedAtAction(nameof(GetLocation), new { id = location.LocationId }, locationReadOnlyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception POST in {nameof(PostLocation)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    _logger.LogWarning($"Location with ID {id} not found in {nameof(DeleteLocation)}");
                    return NotFound();
                }

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception DELETE in {nameof(DeleteLocation)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.LocationId == id);
        }
    }
}
