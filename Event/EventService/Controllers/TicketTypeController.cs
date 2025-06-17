using EventService.Interface;
using EventService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketTypeController : ControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;

        public TicketTypeController(ITicketTypeService service)
        {
            _ticketTypeService = service;
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddTicketType([FromBody] AddTicketTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ticketTypeService.AddTicketTypeAsync(dto);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.CreatedTicket);
        }

        [HttpPut("[action]/{ticketId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTicketType(int ticketId, [FromBody] UpdateTicketTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ticketTypeService.UpdateTicketTypeAsync(ticketId, dto);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok("Ticket type updated successfully.");
        }
    }
}
