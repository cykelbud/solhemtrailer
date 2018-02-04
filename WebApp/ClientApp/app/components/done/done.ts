import { Router } from "aurelia-router";
import { autoinject } from "aurelia-framework";

@autoinject
export class Done {

    constructor(private router : Router){
        
    }


    book(){
        this.router.navigate('');
    }

}