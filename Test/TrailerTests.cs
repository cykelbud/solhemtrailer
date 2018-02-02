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
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddDays(1).Ticks,
                }),
                Then(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddDays(1).Ticks,
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
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddDays(1).Ticks,
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
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddDays(1).Ticks,
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddDays(1).Ticks,
                }),
                ThenFailWith<BookingAlreadyExistsException>()
            );
        }


        [Fact]
        public void MakeBooking_directly_beforeexisting()
        {
            var dateTime = new DateTime(2018, 01, 19).AddHours(1);
            Test(
                Given(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddHours(3).Ticks,
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(-3).Ticks,
                    End = dateTime.Ticks,
                }),
                Then(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(-3).Ticks,
                    End = dateTime.Ticks,
                })
            );
        }
        [Fact]
        public void MakeBooking_directly_after()
        {
            var dateTime = new DateTime(2018, 01, 19).AddHours(1);
            Test(
                Given(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddHours(3).Ticks,
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(3).Ticks,
                    End = dateTime.AddHours(6).Ticks,
                }),
                Then(new TrailerBookedEvent()
                    {
                        BookingId = 1,
                        Id = testId,
                        Start = dateTime.AddHours(3).Ticks,
                        End = dateTime.AddHours(6).Ticks,
                    })
            );
        }

        [Fact]
        public void MakeBooking_overlap_existingstart()
        {
            var dateTime = new DateTime(2018, 01, 19).AddHours(1);
            Test(
                Given(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddHours(3).Ticks,
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(-1).Ticks,
                    End = dateTime.AddHours(4).Ticks,
                }),
                ThenFailWith<BookingAlreadyExistsException>()
            );
        }


        [Fact]
        public void MakeBooking_overlap_existingend()
        {
            var dateTime = new DateTime(2018, 01, 19).AddHours(1);
            Test(
                Given(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.Ticks,
                    End = dateTime.AddHours(3).Ticks,
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(2).Ticks,
                    End = dateTime.AddHours(5).Ticks,
                }),
                ThenFailWith<BookingAlreadyExistsException>()
            );
        }



    }
    
    
}
