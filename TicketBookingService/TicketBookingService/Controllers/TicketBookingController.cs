using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketBookingService.Interfaces;
using TicketBookingService.Models;

namespace TicketBookingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketBookingController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketBookingController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("[action]/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableTickets(int eventId)
        {
            var result = await _ticketService.GetAvailableTicketsAsync(eventId);

            if (!result.Success)
                return StatusCode(500, result.ErrorMessage);

            return Ok(result.Tickets);
        }

        [HttpPost("reserve")]
        [Authorize]
        public async Task<IActionResult> ReserveTicket([FromBody] ReserveTicketRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ticketService.ReserveTicketAsync(request);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Response);
        }

        [HttpPost("confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmTicket([FromBody] ConfirmTicketRequest request)
        {
            var result = await _ticketService.ConfirmTicketAsync(request);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Response);
        }
    }
}
