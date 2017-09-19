import { autoinject } from 'aurelia-framework';
import {Config, Rest} from 'aurelia-api';


@autoinject
export class BookingService {
    apiEndpoint : Rest;

    constructor(config: Config) {
        this.apiEndpoint = config.getEndpoint('api');
    }
}