import { autoinject } from 'aurelia-framework';
import { Config, Rest } from 'aurelia-api';
import { HttpClient } from 'aurelia-fetch-client';

@autoinject
export class Trailer {
    days: Array<Day>;
    api: Rest;
    slots: Array<ISlot[]>;
    bookings: IBooking[];

    constructor(config: Config) {
        this.api = config.getEndpoint('trailer');
    }

    // constructor(private http: HttpClient) {
    // }

    async attached() {


        // let result = await this.http.fetch('api/trailer/slots');
        // let slots = await result.json() as Promise<ISlot[]>;


        this.slots = await this.api.find('slot');

        let schema = this.groupBy(this.slots, 'Date');

        console.log(schema);
        console.log(this.slots);

   
        // let day = new Day();

        // let occations = Array<Occation>({time: "06-09"}, {time: "09-12"}, {time: "12-15"}, {time: "15-18"}, {time: "18-06"});
        // day.occations = occations;
        // this.days = new Array<Day>();
        // this.days.push(day);

        // day.displayText = await this.getItem();


    }

    public async add(slot: ISlot) {
        let request: IBookRequest = {
            SlotId: slot.BookingId,
            Email: "mail@mail.com",
            Phone: "1234567890",
            EndDate: slot.EndTime,
            StartDate: slot.StartTime
        }
        await this.api.post('booking', request);
    }

    public async getAll() {
        this.bookings = await this.api.find('booking');
    }


    public getJson(obj: any){
        return JSON.stringify(obj);
    }


    async getItem(): Promise<string> {
        let item = await this.api.find('GetBookableItemList/cc2ea8334e894cb2a9c9452efadbeb90');
        return <string>item[0].Id;
    }

    groupBy = function (xs: any, key: any) {
        return xs.reduce(function (rv: any, x: any) {
            (rv[x[key]] = rv[x[key]] || []).push(x);
            return rv;
        }, {});
    };

}

interface IBookRequest {
    SlotId: number;
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

class Day {
    displayText: string;
    occations: Array<Occation> = new Array<Occation>();
}

class Occation {
    time: string;
}