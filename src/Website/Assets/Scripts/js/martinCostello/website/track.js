// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/*
 * Tracks an analytics event.
 * @param {string} category - The event category.
 * @param {string} action - The event action.
 * @param {string} label - The event label.
 * @param {string} [value] - The optional event value.
 * @param {object} [fields] - The optional event data.
 * @returns {Boolean} - Whether the analytics event was tracked.
 */
martinCostello.website.track = function (category, action, label, value, fields) {
    if ("ga" in window) {
        ga("send", "event", category, action, label, value, fields);
        return true;
    } else {
        return false;
    }
};
