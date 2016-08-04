/*
 * Defines the namespace for debugging.
 */
martinCostello.website.debug = {
    branch: $("meta[name='x-site-branch']").attr("content"),
    revision: $("meta[name='x-site-revision']").attr("content"),
    log: function (message, optionalParams) {
        console.log(message, optionalParams);
    }
};

/*
 * Logs a message.
 * @param {string} message - The message to log.
 * @param {object} [optionalParams] - The optional parameters to log.
 */
martinCostello.website.debug.log = function (message, optionalParams) {
    console.log(message, optionalParams);
};
