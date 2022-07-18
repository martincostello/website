// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

export class Tracking {
    constructor() {}

    public initialize() {
        const elements = document.querySelectorAll(
            'a, button, input, .ga-track-click'
        );
        for (const element of elements) {
            element.addEventListener('click', () => {
                const label =
                    element.getAttribute('data-ga-label') ||
                    element.getAttribute('id');
                if (label) {
                    const category =
                        element.getAttribute('data-ga-category') || 'General';
                    const action =
                        element.getAttribute('data-ga-action') || 'clicked';

                    this.track(category, action, label);
                }
            });
        }
    }

    private track(category: string, action: string, label: string) {
        if ('gtag' in window) {
            const command = 'event';

            const fields = {
                // eslint-disable-next-line @typescript-eslint/naming-convention
                event_category: category,
                // eslint-disable-next-line @typescript-eslint/naming-convention
                event_label: label,
            };

            const theWindow: any = window;
            const gtag = theWindow.gtag;

            if (gtag) {
                gtag(command, action, fields);
            }
        }
    }
}
