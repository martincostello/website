// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.website.tools {

    /**
     * Represents a hash generator.
     */
    export class HashGenerator extends Generator {

        private algorithm: JQuery;
        private format: JQuery;
        private plaintext: JQuery;
        private textContainer: JQuery;
        private text: JQuery;

        /**
         * Initializes a new instance of the HashGenerator class.
         */
        public constructor() {
            super();
        }

        /**
         * Initializes the generator.
         */
        public initialize(): void {

            this.endpoint = $("link[rel='api-hash']").attr("href");

            if (this.endpoint) {

                let container = $("#generate-hash-container");

                // Inputs
                this.algorithm = container.find("#hash-algorithm");
                this.format = container.find("#hash-format");
                this.plaintext = container.find("#hash-plaintext");

                container
                    .find("#generate-hash")
                    .on("click", this.generate);

                // Outputs
                this.textContainer = container.find("#hash-container");
                this.text = container.find("#text-hash");
            }
        }

        /**
         * Generates a the hash for the current values on the page.
         * @param event - The event object.
         */
        private generate = (event: JQueryEventObject): void => {

            event.preventDefault();

            let data = {
                algorithm: this.algorithm.val() || "sha1",
                format: this.format.val() || "base64",
                plaintext: this.plaintext.val() || ""
            };

            let settings = {
                contentType: "application/json",
                data: JSON.stringify(data),
                method: "POST",
                url: this.endpoint
            };

            $.ajax(settings).then((data) => {
                this.text.val(data.hash);
                this.textContainer.removeClass("hidden");
            });
        }
    }
}
