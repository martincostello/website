// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/*
 * Creates a new instance of martinCostello.website.tools.machineKeyGenerator.
 * @param {string} serviceUri - The URI of the machine key generation service.
 * @param {object} container - The container for the machine key generator.
 * @constructor
 * @returns The new martinCostello.website.tools.machineKeyGenerator.
 */
martinCostello.website.tools.machineKeyGenerator = function (serviceUri, container) {

    // Inputs
    this.decryptionAlgorithm = container.find("#key-decryption-algorithm");
    this.validationAlgorithm = container.find("#key-validation-algorithm");

    this.button = container.find("#generate-machine-key");
    this.button.on("click", $.proxy(this, "onclick"));

    // Outputs
    this.textContainer = container.find("#machine-key-container");
    this.text = container.find("#machine-key-xml");

    this.serviceUri = serviceUri;
};

/*
 * Handles the onclick event of the machine key generation button.
 */
martinCostello.website.tools.machineKeyGenerator.prototype.onclick = function () {
    martinCostello.website.track("tools", "clicked", "Generate machine key");
    this.generate().then($.proxy(this, "update"));
    return false;
};

/*
 * Generates a new machine key.
 * @returns {object} A Promise that returns the response from the machine key generation service.
 */
martinCostello.website.tools.machineKeyGenerator.prototype.generate = function () {

    var query = $.param({
        decryptionAlgorithm: this.decryptionAlgorithm.val(),
        validationAlgorithm: this.validationAlgorithm.val()
    });

    var uri = this.serviceUri + "?" + query;

    return $.get(uri);
};

/*
 * Updates the container with the generated machine key.
 * @param {object} response - The service response containing the generated machine key XML.
 */
martinCostello.website.tools.machineKeyGenerator.prototype.update = function (reponse) {
    this.text.text(reponse.machineKeyXml);
    this.textContainer.removeClass("hidden");
};

(function () {
    var keyEndpoint = $("link[rel='api-machine-key']").attr("href");
    if (keyEndpoint) {
        var keyGenerator = new martinCostello.website.tools.machineKeyGenerator(keyEndpoint, $("#generate-machine-key-container"));
    }
})();
