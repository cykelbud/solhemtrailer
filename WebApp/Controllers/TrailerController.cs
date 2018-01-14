using System;
using System.Collections.Generic;
using Core;
using Edument.CQRS;
using Microsoft.AspNetCore.Mvc;
using Trailer;

namespace solhemtrailer.Controllers
{

    [Route("api/[controller]")]
    public class TrailerController : Controller
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IScheduleQueries _scheduleQueries;

        public TrailerController(IMessageDispatcher dispatcher, IScheduleQueries scheduleQueries)
        {
            _dispatcher = dispatcher;
            _scheduleQueries = scheduleQueries;
        }


        [HttpGet("[action]")]
        public IEnumerable<ScheduleSlot> Slots([FromBody]TimespanRequest request = null)
        {
            if (request == null)
            {
                request = new TimespanRequest()
                {
                    StartDate = DateTime.Now.ToShortDateString(),
                    EndDate = DateTime.Now.AddDays(1).ToShortDateString()
                };
            }

            var startDate = DateTime.Parse(request.StartDate);
            var endDate = DateTime.Parse(request.EndDate);
            var slots = _scheduleQueries.GetBookingSchedule(Guid.Empty, startDate, endDate);

            return slots;
        }

        [HttpPost]
        public JsonResult Book(BookSlotRequest request)
        {
            _dispatcher.SendCommand(new BookTrailerCommand()
            {
                BookingId = request.SlotId,
                TrailerId = Guid.Empty,
                Start = request.StartDate,
                End = request.EndDate,
                Email = request.Email,
                Phone = request.Phone,
            });
            return Json(Ok());
        }



    }

    public class BookSlotRequest
    {
        public long SlotId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class TimespanRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }


}