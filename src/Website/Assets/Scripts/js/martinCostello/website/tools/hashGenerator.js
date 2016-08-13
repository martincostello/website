/*
 * Creates a new instance of martinCostello.website.tools.hashGenerator.
 * @param {string} serviceUri - The URI of the hash generation service.
 * @param {object} container - The container for the hash generator.
 * @constructor
 * @returns The new martinCostello.website.tools.hashGenerator.
 */
martinCostello.website.tools.hashGenerator = function (serviceUri, container) {

    // Inputs
    this.algorithm = container.find("#hash-algorithm");
    this.format = container.find("#hash-format");
    this.plaintext = container.find("#hash-plaintext");

    this.button = container.find("#generate-hash");
    this.button.on("click", $.proxy(this, "onclick"));

    // Outputs
    this.textContainer = container.find("#hash-container");
    this.text = container.find("#text-hash");

    this.serviceUri = serviceUri;
};

/*
 * Handles the onclick event of the hash generation button.
 */
martinCostello.website.tools.hashGenerator.prototype.onclick = function () {
    martinCostello.website.track("tools", "clicked", "Generate hash");
    this.generate().then($.proxy(this, "update"));
    return false;
};

/*
 * Generates a new hash.
 * @returns {object} A Promise that returns the response from the hash generation service.
 */
martinCostello.website.tools.hashGenerator.prototype.generate = function () {

    var data = {
        algorithm: this.algorithm.val() || "sha1",
        format: this.format.val() || "base64",
        plaintext: this.plaintext.val() || ""
    };

    var settings = {
        contentType: "application/json",
        data: JSON.stringify(data),
        method: "POST",
        url: this.serviceUri
    };

    return $.ajax(settings);
};

/*
 * Updates the container with the generated hash.
 * @param {object} response - The service response containing the generated hash.
 */
martinCostello.website.tools.hashGenerator.prototype.update = function (reponse) {
    this.text.val(reponse.hash);
    this.textContainer.removeClass("hidden");
};

(function () {
    var hashEndpoint = $("link[rel='api-hash']").attr("href");
    if (hashEndpoint) {
        var hashGenerator = new martinCostello.website.tools.hashGenerator(hashEndpoint, $("#generate-hash-container"));
    }
})();
