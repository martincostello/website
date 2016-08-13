/*
 * Creates a new instance of martinCostello.website.tools.guidGenerator.
 * @param {string} serviceUri - The URI of the GUID generation service.
 * @param {object} container - The container for the GUID generator.
 * @constructor
 * @returns The new martinCostello.website.tools.guidGenerator.
 */
martinCostello.website.tools.guidGenerator = function (serviceUri, container) {

    // Inputs
    this.format = container.find("#guid-format");
    this.uppercase = container.find("#guid-uppercase");

    this.button = container.find("#generate-guid");
    this.button.on("click", $.proxy(this, "onclick"));

    // Outputs
    this.text = container.find("#text-guid");

    this.serviceUri = serviceUri;
};

/*
 * Handles the onclick event of the GUID generation button.
 */
martinCostello.website.tools.guidGenerator.prototype.onclick = function () {
    martinCostello.website.track("tools", "clicked", "Generate GUID");
    this.generate().then($.proxy(this, "update"));
    return false;
};

/*
 * Generates a new GUID.
 * @returns {object} A Promise that returns the response from the GUID generation service.
 */
martinCostello.website.tools.guidGenerator.prototype.generate = function () {

    var query = $.param({
        format: this.format.val(),
        uppercase: this.uppercase.is(":checked")
    });

    var uri = this.serviceUri + "?" + query;

    return $.get(uri);
};

/*
 * Updates the container with the generated GUID.
 * @param {object} response - The service response containing the generated GUID.
 */
martinCostello.website.tools.guidGenerator.prototype.update = function (reponse) {
    this.text.val(reponse.guid);
};

(function () {
    var guidEndpoint = $("link[rel='api-guid']").attr("href");
    if (guidEndpoint) {
        var guidGenerator = new martinCostello.website.tools.guidGenerator(guidEndpoint, $("#generate-guid-container"));
    }
})();
