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
                    Start = dateTime,
                    End = dateTime.AddDays(1),
                }),
                Then(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
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
                    Id = testId,
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
                    Id = testId,
                    Start = dateTime,
                    End = dateTime.AddDays(1),
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime,
                    End = dateTime.AddDays(1),
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
                    Start = dateTime,
                    End = dateTime.AddHours(3),
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(-3),
                    End = dateTime,
                }),
                Then(new TrailerBookedEvent()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(-3),
                    End = dateTime,
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
                    Start = dateTime,
                    End = dateTime.AddHours(3),
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(3),
                    End = dateTime.AddHours(6),
                }),
                Then(new TrailerBookedEvent()
                    {
                        BookingId = 1,
                        Id = testId,
                        Start = dateTime.AddHours(3),
                        End = dateTime.AddHours(6),
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
                    Start = dateTime,
                    End = dateTime.AddHours(3),
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(-1),
                    End = dateTime.AddHours(4),
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
                    Start = dateTime,
                    End = dateTime.AddHours(3),
                }),
                When(new BookTrailerCommand()
                {
                    BookingId = 1,
                    Id = testId,
                    Start = dateTime.AddHours(2),
                    End = dateTime.AddHours(5),
                }),
                ThenFailWith<BookingAlreadyExistsException>()
            );
        }



    }
    
    
}
