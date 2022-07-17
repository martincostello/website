// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import moment from 'moment';
import { Tools } from './Tools';
import { Tracking } from './Tracking';

export class App {
    constructor() {}

    async initialize() {
        const element = document.getElementById('build-date');

        if (element) {
            const timestamp = element.getAttribute('data-timestamp');
            const format = element.getAttribute('data-format');

            const value = moment(timestamp, format);

            if (value.isValid()) {
                let text: string = value.fromNow();
                element.textContent = `| Last updated ${text}`;
            }
        }

        const tracking = new Tracking();
        tracking.initialize();

        const tools = new Tools();
        tools.initialize();

        setTimeout(() => {
            const images = document.querySelectorAll('img.lazy');
            for (const image of images) {
                image.setAttribute('src', image.getAttribute('data-original'));
            }
        }, 500);

        if ('serviceWorker' in navigator) {
            (navigator as any).serviceWorker
                .register('/service-worker.js')
                .then(() => {})
                .catch((err: any) =>
                    console.warn('Failed to register Service Worker: ', err)
                );
        }
    }
}
