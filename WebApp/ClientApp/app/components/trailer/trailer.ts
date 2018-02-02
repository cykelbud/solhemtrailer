import { autoinject, bindable, computedFrom } from 'aurelia-framework';
import { Config, Rest } from 'aurelia-api';
import { HttpClient } from 'aurelia-fetch-client';
import * as moment from 'moment';
import * as $ from "jquery";


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

    constructor(config: Config) {
        this.api = config.getEndpoint('trailer');
        this.currentStep = Step.Select;
        this.bookRequest = {
            Email: '',
            Phone: '',
            EndDate: 0,
            StartDate: 0
        };
        moment.locale('sv');
    }

    async attached() {

        //$('.f1 fieldset:first').fadeIn('slow');

        this.currentWeekStart = this.getThisWeekStart();
        await this.getWeekSchedule(this.currentWeekStart);
        await this.getAllBookings();



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
            await this.api.post('booking', this.bookRequest);
            slot.IsAvailable = false;

        } catch(error){
            throw error;
        }
        finally {
            this.isBooking = false;
            this.selectedSlot = new Slot;
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
        let nextWeekStart = moment(weekStart).add(8, 'days').format('YYYY-MM-DD');
        return nextWeekStart;
    }

    getPreviousWeekStart(weekStart: string) {
        let nextWeekStart = moment(weekStart).add(-8, 'days').format('YYYY-MM-DD');
        return nextWeekStart;
    }

    async getWeekSchedule(weekStart: string) {
        let dateFirstDayOfWeek = weekStart;
        let dateLastDayOfWeek = moment(weekStart).add(7, 'days').format('YYYY-MM-DD');

        this.scheduleDateDisplayText = dateFirstDayOfWeek + ' till ' + dateLastDayOfWeek;

        let search: IScheduleCriteria =
            {
                StartDate: dateFirstDayOfWeek,
                EndDate: dateLastDayOfWeek
            };
        this.slots = await this.api.find('slot', search);
    }

    public async getNextWeekSchedule() {
        let nextWeekStart = this.getNextWeekStart(this.currentWeekStart);
        await this.getWeekSchedule(nextWeekStart);
        this.currentWeekStart = nextWeekStart;
    }

    public async getPreviousWeekSchedule() {
        let prevWeekStart = this.getPreviousWeekStart(this.currentWeekStart);
        await this.getWeekSchedule(prevWeekStart);
        this.currentWeekStart = prevWeekStart;
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

class Slot {
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
    Start: number;
    End: number;
}

interface IScheduleCriteria {
    StartDate: string;
    EndDate: string;
} 
