// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.website {

    /**
     * Represents the class for analytics tracking.
     */
    export class Tracking {

        /**
         * Tracks an analytics event.
         * @param {string} category - The event category.
         * @param {string} action - The event action.
         * @param {string} label - The event label.
         * @returns {boolean} - Whether the analytics event was tracked.
         */
        public static track(category: string, action: string, label: string): boolean {

            let tracked = false;

            if ("ga" in window && ga) {

                let command = "send";
                let fields = {
                    hitType: "event",
                    eventCategory: category,
                    eventAction: action,
                    eventLabel: label
                };

                ga(command, fields);

                tracked = true;
            }

            return tracked;
        }
    }
}

(() => {
    $("a, button, input, .ga-track-click").on("click", (e: JQueryEventObject): void => {

        const element = $(e.target);
        const label = element.attr("data-ga-label") || element.attr("id");

        if (label) {

            const category = element.attr("data-ga-category") || "General";
            const action = element.attr("data-ga-action") || "clicked";

            martinCostello.website.Tracking.track(
                category,
                action,
                label);
        }
    });
})();
