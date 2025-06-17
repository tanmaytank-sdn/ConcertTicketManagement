using EventService.Interface;
using EventService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUpdateEvent([FromBody] AddUpdateEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _eventService.AddOrUpdateEventAsync(dto);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return dto.EventId > 0 ? Ok(result.Result) : CreatedAtAction(null, new { id = ((EventResponseDto)result.Result).EventId }, result.Result);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents()
        {
            var result = await _eventService.GetAllEventsAsync();
            return Ok(result);
        }

        [HttpGet("[action]/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventById(int id)
        {
            var result = await _eventService.GetEventByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("[action]/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventTicketType(int id)
        {
            var result = await _eventService.GetEventTicketTypesAsync(id);
            if (result == null || result.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("[action]/{eventId}/{ticketTypeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventTicketType(int eventId, int ticketTypeId)
        {
            var result = await _eventService.GetSpecificTicketTypeAsync(eventId, ticketTypeId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("[action]/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await _eventService.DeleteEventAsync(id);
            if (!result.Success)
                return NotFound(result.ErrorMessage);

            return NoContent();
        }
    }
}
