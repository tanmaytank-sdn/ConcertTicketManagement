using Microsoft.AspNetCore.Mvc;
using VenueService.Interface;
using VenueService.RequestDTO;

namespace VenueService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenueController : ControllerBase
    {
        private readonly IVenueService _venueService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VenueController(IVenueService venueService, IHttpContextAccessor httpContextAccessor)
        {
            _venueService = venueService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetVenueById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    Message = "Invalid venue ID. It must be a positive integer greater than zero."
                });
            }
            else
            {
                var result = await _venueService.GetVenueByIdAsync(id);

                if (!result.Success)
                    return Forbid(result.ErrorMessage);

                return Ok(result.Data);
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllVenues()
        {
            var result = await _venueService.GetAllVenuesAsync();

            if (!result.Success)
                return Forbid(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddOrUpdateVenueAsync(AddOrUpdateVenueRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    Message = "Validation failed.",
                    Errors = errors
                });
            }
            else
            {
                var result = await _venueService.AddOrUpdateVenueAsync(request);

                if (!result.Success)
                    return BadRequest(result.ErrorMessage);

                return Ok("Venue added/updated successfully.");
            }
        }
    }
}
