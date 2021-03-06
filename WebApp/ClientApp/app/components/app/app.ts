import { Aurelia, PLATFORM } from 'aurelia-framework';
import { Router, RouterConfiguration } from 'aurelia-router';

export class App {
    router: Router;

    configureRouter(config: RouterConfiguration, router: Router) {
        config.title = 'Aurelia';
        config.map([{
            route: [ 'home' ],
            name: 'home',
            settings: { icon: 'home' },
            moduleId: PLATFORM.moduleName('../home/home'),
            nav: true,
            title: 'Home'
        }, {
            route: 'counter',
            name: 'counter',
            settings: { icon: 'education' },
            moduleId: PLATFORM.moduleName('../counter/counter'),
            nav: true,
            title: 'Counter'
        }, {
            route: 'fetch-data',
            name: 'fetchdata',
            settings: { icon: 'th-list' },
            moduleId: PLATFORM.moduleName('../fetchdata/fetchdata'),
            nav: true,
            title: 'Fetch data'
        }, {
            route: [ '', 'trailer' ],
            name: 'trailer',
            settings: { icon: 'time' },
            moduleId: PLATFORM.moduleName('../trailer/trailer'),
            nav: true,
            title: 'Boka släpet'
        }, {
            route: 'done',
            name: 'done',
            settings: { icon: 'time' },
            moduleId: PLATFORM.moduleName('../done/done'),
            nav: true,
            title: 'Tack'
        }, {
            route: 'book',
            name: 'book',
            settings: { icon: 'time' },
            moduleId: PLATFORM.moduleName('../book/book'),
            nav: true,
            title: 'Book trailer'
        }]);

        this.router = router;
    }
}
