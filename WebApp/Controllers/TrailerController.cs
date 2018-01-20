using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Mvc;
using ReadModels;
using Trailer;

namespace solhemtrailer.Controllers
{

    [Route("api/[controller]")]
    public class TrailerController : Controller
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IScheduleQueries _scheduleQueries;
        private readonly Guid _trailerId;

        public TrailerController(IMessageDispatcher dispatcher, IScheduleQueries scheduleQueries)
        {
            _dispatcher = dispatcher;
            _scheduleQueries = scheduleQueries;
            _trailerId = Constants.TrailerId;
        }


        [HttpGet("slot")]
        public async Task<IEnumerable<List<ScheduleSlot>>> Slots(TimespanRequest request = null)
        {
            if (request == null)
            {
                request = new TimespanRequest()
                {
                    StartDate = DateTime.Now.ToShortDateString(),
                    EndDate = DateTime.Now.AddDays(7).ToShortDateString()
                };
            }

            var startDate = DateTime.Parse(request.StartDate);
            var endDate = DateTime.Parse(request.EndDate);
            var slots = _scheduleQueries.GetBookingSchedule(_trailerId, startDate, endDate).ToList();

            var slotsBy = slots.GroupBy(s => s.Date).Select(k => k.ToList());


            return slotsBy;
        }

        [HttpPost("booking")]
        public JsonResult Book([FromBody] BookSlotRequest request)
        {
            _dispatcher.SendCommand(new BookTrailerCommand()
            {
                Id = _trailerId,
                BookingId = request.StartDate,
                Start = request.StartDate,
                End = request.EndDate,
                Email = request.Email,
                Phone = request.Phone,
            });
            return Json(Ok());
        }

        [HttpDelete("booking/{bookingId}")]
        public JsonResult DeleteBooking(long bookingId)
        {
            _dispatcher.SendCommand(new CancelBookingCommand()
            {
                Id = _trailerId,
                BookingId = bookingId,
            });
            return Json(Ok());
        }

        [HttpGet("booking")]
        public IEnumerable<Booking> GettAllBookings()
        {
            return _scheduleQueries.GetAll();
        }

    }

    public class BookSlotRequest
    {
        public long SlotId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class TimespanRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }


}