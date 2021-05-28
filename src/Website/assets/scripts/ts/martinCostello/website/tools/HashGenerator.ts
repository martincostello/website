// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.website.tools {

    /**
     * Represents a hash generator.
     */
    export class HashGenerator extends Generator {

        private algorithm: JQuery<Element>;
        private format: JQuery<Element>;
        private plaintext: JQuery<Element>;
        private textContainer: JQuery<Element>;
        private text: JQuery<Element>;

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

            this.endpoint = $("link[name='api-hash']").attr("href");

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
        private generate = (event: any): void => {

            event.preventDefault();

            const data = {
                algorithm: this.algorithm.val() || "sha1",
                format: this.format.val() || "base64",
                plaintext: this.plaintext.val() || ""
            };

            const settings = {
                contentType: "application/json",
                data: JSON.stringify(data),
                method: "POST",
                url: this.endpoint
            };

            $.ajax(settings).then((data) => {
                this.text.val(data.hash);
                this.text.attr("value", data.hash);
                this.textContainer.removeClass("d-none");
            });
        }
    }
}
