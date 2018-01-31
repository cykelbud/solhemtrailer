import { autoinject, bindable } from 'aurelia-framework';
import { Config, Rest } from 'aurelia-api';
import { HttpClient } from 'aurelia-fetch-client';


@autoinject
export class Trailer {

    @bindable picker : any;

    api: Rest;
    slots: Array<ISlot[]>;
    bookings: IBooking[];

    constructor(config: Config) {
        this.api = config.getEndpoint('trailer');
    }

    async attached() {
        let search: IScheduleCriteria =
            {
                StartDate: '2018-01-21',
                EndDate: '2018-01-28'
            };
        this.slots = await this.api.find('slot', search);
        await this.getAllBookings();
    }

    public async add(slot: ISlot) {
        let request: IBookRequest = {
            Email: "mail@mail.com",
            Phone: "1234567890",
            EndDate: slot.EndTime,
            StartDate: slot.StartTime
        };
        await this.api.post('booking', request);
        slot.IsAvailable = false;
        await this.getAllBookings();
    }

    public async getAllBookings() {
        this.bookings = await this.api.find('booking');
    }


    public async cancel(booking: IBooking) {
        await this.api.destroyOne('booking', booking.BookingId);
        await this.getAllBookings();
    }

    public getJson(obj: any) {
        return JSON.stringify(obj);
    }


}

interface IBookRequest {
    StartDate: number;
    EndDate: number;
    Email: string;
    Phone: string;
}

interface ISlot {
    TrailerId: string;
    BookingId: number;
    StartTime: number;
    EndTime: number;
    IsAvailable: boolean;
    Date: string;
}

interface IBooking {
    BookingId: number;
    TrailerId: string;
    Start: number;
    End: number;
}

interface IScheduleCriteria {
    StartDate: string;
    EndDate: string;
} 
