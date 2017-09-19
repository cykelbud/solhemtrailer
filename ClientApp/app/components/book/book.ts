import { autoinject } from 'aurelia-framework';
import {Config, Rest} from 'aurelia-api';

@autoinject
export class Book {
    days : Array<Day>;
    apiEndpoint : Rest;

    constructor(config: Config) {
        this.apiEndpoint = config.getEndpoint('events');


    }

    async attached(){

        let day = new Day();

        let occations = Array<Occation>({time: "06-09"}, {time: "09-12"}, {time: "12-15"}, {time: "15-18"}, {time: "18-06"});
        day.occations = occations;
        this.days = new Array<Day>();
        this.days.push(day);
        
        day.displayText = await this.getItem();
        

    }


    async getItem() : Promise<string> {
        let item = await this.apiEndpoint.find('GetBookableItemList/cc2ea8334e894cb2a9c9452efadbeb90');
        return <string>item[0].Id;
    }
        
}



class Day {
    displayText : string;
    occations : Array<Occation> = new Array<Occation>();
}

class Occation {
    time : string;
}