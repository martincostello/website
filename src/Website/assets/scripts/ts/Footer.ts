// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

declare var moment: any;

(() => {
    $(document).ready((): void => {

        const element = $("#build-date");

        if (element && "moment" in window) {

            const timestamp = element.attr("data-timestamp");
            const format = element.attr("data-format");

            const value = moment(timestamp, format);

            if (value.isValid()) {
                let text: string = value.fromNow();
                element.text(`| Last updated ${text}`);
            }
        }
    });
})();
