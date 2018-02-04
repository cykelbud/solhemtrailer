import 'isomorphic-fetch';
import { Aurelia, PLATFORM } from 'aurelia-framework';
import { HttpClient } from 'aurelia-fetch-client';
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap';
//import {Config} from 'aurelia-config';
import {Config} from 'aurelia-api';

declare const IS_DEV_BUILD: boolean; // The value is supplied by Webpack during the build

export function configure(aurelia: Aurelia) {
    aurelia.use.standardConfiguration();
    
    // aurelia.use.plugin('aurelia-config', (configure : Config) => {
    //     return configure([
    //       'aurelia-api']);

        // // Add this:
        aurelia.use.plugin(PLATFORM.moduleName('aurelia-api'), (config : Config) => {
            config.registerEndpoint('trailer', 'api/trailer/');
        })
        .plugin(PLATFORM.moduleName('aurelia-validation'));


    if (IS_DEV_BUILD) {
        aurelia.use.developmentLogging();
    }

    aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName('app/components/app/app')));
}
