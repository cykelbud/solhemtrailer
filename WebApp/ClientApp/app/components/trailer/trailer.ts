import { autoinject } from 'aurelia-framework';
import {Config, Rest} from 'aurelia-api';
import { HttpClient } from 'aurelia-fetch-client';

@autoinject
export class Trailer {
    days : Array<Day>;
    api : Rest;

    constructor(config: Config) {
        this.api = config.getEndpoint('trailer');
    }

    // constructor(private http: HttpClient) {
    // }

    async attached(){


        // let result = await this.http.fetch('api/trailer/slots');
        // let slots = await result.json() as Promise<ISlot[]>;


        let slots : ISlot[] = await this.api.find('slots');


         console.log(slots);


        // let day = new Day();

        // let occations = Array<Occation>({time: "06-09"}, {time: "09-12"}, {time: "12-15"}, {time: "15-18"}, {time: "18-06"});
        // day.occations = occations;
        // this.days = new Array<Day>();
        // this.days.push(day);
        
        // day.displayText = await this.getItem();
        

    }

    public async add()  {
        let request : IBookRequest = {
            SlotId : 1,
            Email : "mail",
            EndDate : '2018-01-19',
            StartDate : '2018-01-18',
            Phone : '1234567'
        }
        await this.api.post('book', request);
    }



    async getItem() : Promise<string> {
        let item = await this.api.find('GetBookableItemList/cc2ea8334e894cb2a9c9452efadbeb90');
        return <string>item[0].Id;
    }
        
}

interface IBookRequest {
    SlotId : number;
    StartDate : string;
    EndDate : string;
    Email : string;
    Phone : string;
}

interface ISlot {
    TrailerId : string;
    BookingId : number;
    StartTime : Date;
    EndTime : Date;
    IsAvailable : boolean;
}

class Day {
    displayText : string;
    occations : Array<Occation> = new Array<Occation>();
}

class Occation {
    time : string;
}