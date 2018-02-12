import { autoinject, bindable, computedFrom } from 'aurelia-framework';
import { Config, Rest } from 'aurelia-api';
import { HttpClient } from 'aurelia-fetch-client';
import { validationMessages, ValidationRules, ValidationController, ValidationControllerFactory, Validator } from 'aurelia-validation';
import * as moment from 'moment';
import * as $ from "jquery";
import { Router } from 'aurelia-router';


export enum Step {
    Select = 1,
    Book = 2,
    Done = 3
}

@autoinject
export class Trailer {

    api: Rest;
    slots: Array<Slot[]>;
    bookings: IBooking[];
    @bindable scheduleDateDisplayText: string;
    @bindable currentStep: number;
    selectedSlot: Slot;
    selectedStart: string;
    currentWeekStart: string;
    bookRequest: IBookRequest;
    isBooking: boolean;
    controller : ValidationController;
    loadingNext :boolean;
    loadingPrev :boolean;
    bookingsRequest : IBookingsRequest;
    notFoundMessage : string;
    canceling : boolean;
    

    constructor(
        config: Config, 
        factory: ValidationControllerFactory, 
        private validator: Validator,
        private router: Router) {

        this.api = config.getEndpoint('trailer');
        this.controller = factory.createForCurrentScope(validator);
        this.currentStep = Step.Select;
        this.bookRequest = {
            Email: '',
            Phone: '',
            EndDate: 0,
            StartDate: 0
        };
        this.bookingsRequest = { Phone:''};
        moment.locale('sv');
    }

    async attached() {

        $('.f1 fieldset:first').fadeIn('slow');

        this.currentWeekStart = this.getThisWeekStart();
        await this.getWeekSchedule(this.currentWeekStart);
        await this.getAllBookings();

        ValidationRules
            // .ensure((r :IBookRequest) => r.Email)
            // .required()
            // .withMessage('Ange en giltig epostadress')
            // .email()
            // .withMessage('Ange en giltig epostadress')

            .ensure((r:IBookRequest) => r.Phone)
            .required()
            .withMessage('Ange ditt mobilnummer som du kommer ringa upp låset ifrån. ex. 0701234565 endast siffror')
            .matches(new RegExp('^07+[0-9]{8}$'))
            .withMessage('Ange ditt mobilnummer som du kommer ringa upp låset ifrån. ex. 0701234565 endast siffror')
            
            .ensure((r:IBookRequest) => r.StartDate)
            .required()
            .satisfies((start:number, req:IBookRequest) => {
                return start > 0;
            })
            .withMessage('Måste välja en tid!')

            .ensure((r:IBookRequest) => r.EndDate)
            .required()
            .satisfies((end:number, req:IBookRequest) => {
                return end > 0;
            })
            .withMessage('Måste välja en tid!')

            .on(this.bookRequest);

        ValidationRules
            .ensure((req:IBookingsRequest) => req.Phone)
            .required()
            .withMessage('Måste ha mobilnummer')
            .matches(new RegExp('^07+[0-9]{8}$'))
            .withMessage('Ange mobilnummer 10 siffror utan steck (07XXXXXXXXX)')
            .on(this.bookingsRequest);

        // next step
        // $('.f1 .btn-next').on('click', function () {
        //     var parent_fieldset = $(this).parents('fieldset');
        //     var next_step = true;
        //     // navigation steps / progress steps
        //     var current_active_step = $(this).parents('.f1').find('.f1-step.active');
        //     var progress_line = $(this).parents('.f1').find('.f1-progress-line');

        //     // fields validation
        //     parent_fieldset.find('input[type="text"], input[type="password"], textarea').each(function () {
        //         if ($(this).val() == "") {
        //             $(this).addClass('input-error');
        //             next_step = false;
        //         }
        //         else {
        //             $(this).removeClass('input-error');
        //         }
        //     });
        //     // fields validation

        //     if (next_step) {
        //         parent_fieldset.fadeOut(400, function () {
        //             // change icons
        //             current_active_step.removeClass('active').addClass('activated').next().addClass('active');
        //             // progress bar
        //             bar_progress(progress_line, 'right');
        //             // show next step
        //             $(this).next().fadeIn();
        //             // scroll window to beginning of the form
        //             scroll_to_class($('.f1'), 20);
        //         });
        //     }

        // });

        // previous step
        // $('.f1 .btn-previous').on('click', function () {
        //     // navigation steps / progress steps
        //     var current_active_step = $(this).parents('.f1').find('.f1-step.active');
        //     var progress_line = $(this).parents('.f1').find('.f1-progress-line');

        //     $(this).parents('fieldset').fadeOut(400, function () {
        //         // change icons
        //         current_active_step.removeClass('active').prev().removeClass('activated').addClass('active');
        //         // progress bar
        //         bar_progress(progress_line, 'left');
        //         // show previous step
        //         $(this).prev().fadeIn();
        //         // scroll window to beginning of the form
        //         scroll_to_class($('.f1'), 20);
        //     });
        // });



    }

    public async select(slot: Slot) {
        if (this.selectedSlot) {
            this.selectedSlot.IsSelected = false;
        }

        slot.IsSelected = true;
        this.selectedSlot = slot;
        this.selectedStart = moment(this.selectedSlot.StartUtc).format('YYYY-MM-DD HH:mm');
    }

    public async book() {
        try {
            this.isBooking = true;
            let slot = this.selectedSlot;
            this.bookRequest.StartDate = slot.StartTime;
            this.bookRequest.EndDate = slot.EndTime;

            let result = await this.controller.validate({object: this.bookRequest});
            if (!result.valid){
                return;
            }

            await this.api.post('booking', this.bookRequest);
            slot.IsAvailable = false;
            this.router.navigate('done');

        } catch(error){
            throw error;
        }
        finally {
            this.isBooking = false;
        }
    }

    public next(): void {

        if (!this.selectedSlot) {
            return;
        }

        let step = this.currentStep;
        if (step == 3) {
            this.currentStep = 1;
            return;
        }
        this.currentStep = step + 1;
    }

    public previous(): void {
        let step = this.currentStep;
        if (step == 1) {
            this.currentStep = 3;
            return;
        }
        this.currentStep = step - 1;
    }


    @computedFrom('isBooking', 'selectedSlot', 'selectedSlot.IsAvailable')
    get canBook(): boolean {
        return !this.isBooking && this.selectedSlot && this.selectedSlot.IsAvailable;
    }

    displayDate(date: string): string {
        let dayOfWeek = moment(date).day();
        return moment.weekdaysShort(dayOfWeek) + ' ' + date;
    }

    getThisWeekStart(): string {
        let day = moment().isoWeekday() - 1;
        let dateFirstDayOfWeek = moment().add(-day, 'days').format('YYYY-MM-DD');
        return dateFirstDayOfWeek;
    }

    getNextWeekStart(weekStart: string) {
        let nextWeekStart = moment(weekStart).add(7, 'days').format('YYYY-MM-DD');
        return nextWeekStart;
    }

    getPreviousWeekStart(weekStart: string) {
        let nextWeekStart = moment(weekStart).add(-7, 'days').format('YYYY-MM-DD');
        return nextWeekStart;
    }

    async getWeekSchedule(weekStart: string) {
        let week = moment(weekStart).isoWeek();
        let dateFirstDayOfWeek = weekStart;
        let dateLastDayOfWeek = moment(weekStart).add(7, 'days').format('YYYY-MM-DD');

        this.scheduleDateDisplayText = 'Vecka ' + week + ': ' + dateFirstDayOfWeek + ' till ' + dateLastDayOfWeek;

        let search: IScheduleCriteria =
            {
                StartDate: dateFirstDayOfWeek,
                EndDate: dateLastDayOfWeek
            };
        this.slots = await this.api.find('slot', search);
    }

    public async getNextWeekSchedule() {
        this.loadingNext = true;
        let nextWeekStart = this.getNextWeekStart(this.currentWeekStart);
        await this.getWeekSchedule(nextWeekStart);
        this.currentWeekStart = nextWeekStart;
        this.loadingNext = false;
    }

    public async getPreviousWeekSchedule() {
        this.loadingPrev = true;
        let prevWeekStart = this.getPreviousWeekStart(this.currentWeekStart);
        await this.getWeekSchedule(prevWeekStart);
        this.currentWeekStart = prevWeekStart;
        this.loadingPrev = false;
    }


    public async getAllBookings() {
        this.bookings = await this.api.find('booking');
    }


    public async cancel(booking: IBooking) {
        this.canceling = true;
        await this.api.destroyOne('booking', booking.BookingId);
        await this.getAllBookings();
        this.canceling = false;
    }

    public getJson(obj: any) {
        return JSON.stringify(obj);
    }


    public async getBookings(){
        this.notFoundMessage = '';
        
        var val = await this.controller.validate({object:this.bookingsRequest});
        if (!val.valid){
            return;
        }
        this.bookings = await this.api.find('bookings', this.bookingsRequest.Phone);
        if (this.bookings.length < 1){
            this.notFoundMessage = 'Inga bokningar hittades';
        }
        
    }

    public formatBooking(booking : IBooking){
        return 'Tel: ' + booking.Phone + ' Tid: ' + booking.StartTime + ' - ' + booking.EndTime;
    }


    // scroll_to_class(element_class: JQuery<HTMLElement>, removed_height: any) {
    //     let offset = element_class.offset();

    //     if (!offset) {
    //         return;
    //     }

    //     var scroll_to = offset.top - removed_height;
    //     if ($(window).scrollTop() != scroll_to) {
    //         $('html, body').stop().animate({ scrollTop: scroll_to }, 0);
    //     }
    // };

    // bar_progress(progress_line_object: any, direction: any) {
    //     var number_of_steps = progress_line_object.data('number-of-steps');
    //     var now_value = progress_line_object.data('now-value');
    //     var new_value = 0;
    //     if (direction == 'right') {
    //         new_value = now_value + (100 / number_of_steps);
    //     }
    //     else if (direction == 'left') {
    //         new_value = now_value - (100 / number_of_steps);
    //     }
    //     progress_line_object.attr('style', 'width: ' + new_value + '%;').data('now-value', new_value);
    // }

}

interface IBookRequest {
    StartDate: number;
    EndDate: number;
    Email: string;
    Phone: string;
}

interface IBookingsRequest {
    Phone: string;
}

interface Slot {
    TrailerId: string;
    BookingId: number;
    StartTime: number;
    StartUtc: string;
    EndTime: number;
    EndUtc: string;
    IsAvailable: boolean;
    Date: string;
    IsSelected: boolean;
}


interface IBooking {
    BookingId: number;
    TrailerId: string;
    Phone: number;
    StartTime: string;
    EndTime: string;
}

interface IScheduleCriteria {
    StartDate: string;
    EndDate: string;
} 
