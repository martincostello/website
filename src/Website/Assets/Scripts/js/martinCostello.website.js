/*
 * Defines the namespace for the website.
 */
martinCostello.website = {
};

/*
 * Tracks an analytics event.
 * @param {string} category - The event category.
 * @param {string} action - The event action.
 * @param {string} label - The event label.
 * @param {string} [value] - The optional event label.
 * @param {object} [fields] - The optional event data.
 */
martinCostello.website.track = function (category, action, label, value, fields) {
    if ("ga" in window) {
        ga("send", "event", category, action, label, value, fields);
    }
};
