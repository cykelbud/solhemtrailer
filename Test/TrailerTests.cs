using System;
using Edument.CQRS;
using Events;
using Trailer;
using Xunit;

namespace Test
{

    public class TrailerTests : BDDTest<TrailerAggregate>
    {
        private Guid testId;

        public TrailerTests() : base()
        {
            testId = Guid.NewGuid();
        }

        [Fact]
        public void BookTrailerCommand()
        {
            var dateTime = DateTime.Now;
            Test(
                Given(),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    TrailerId = testId,
                    Start = dateTime,
                    End = dateTime.AddDays(1),
                }),
                Then(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    TrailerId = testId,
                    Start = dateTime,
                    End = dateTime.AddDays(1),
                })
                );
        }

        [Fact]
        public void CancelBookingCommand()
        {
            var dateTime = DateTime.Now;
            Test(
                Given(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    TrailerId = testId,
                    Start = dateTime,
                    End = dateTime.AddDays(1),
                }),
                When(new CancelBookingCommand()
                {
                    BookingId = 1,
                }),
                Then(new TrailerBookingCanceledEvent()
                {
                    BookingId = 1,
                })
            );
        }

        [Fact]
        public void MakeBookingWhenBookingExists()
        {
            var dateTime = DateTime.Now;
            Test(
                Given(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    TrailerId = testId,
                    Start = dateTime,
                    End = dateTime.AddDays(1),
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    TrailerId = testId,
                    Start = dateTime,
                    End = dateTime.AddDays(1),
                }),
                ThenFailWith<BookingAlreadyExistsException>()
            );
        }
    }
    
    
}
