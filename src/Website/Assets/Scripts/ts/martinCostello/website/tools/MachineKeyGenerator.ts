// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.website.tools {

    /**
     * Represents a machine key generator.
     */
    export class MachineKeyGenerator extends Generator {

        private decryptionAlgorithm: JQuery;
        private validationAlgorithm: JQuery;
        private textContainer: JQuery;
        private text: JQuery;

        /**
         * Initializes a new instance of the MachineKeyGenerator class.
         */
        public constructor() {
            super();
        }

        /**
         * Initializes the generator.
         */
        public initialize(): void {

            this.endpoint = $("link[name='api-machine-key']").attr("href");

            if (this.endpoint) {

                const container = $("#generate-machine-key-container");

                // Inputs
                this.decryptionAlgorithm = container.find("#key-decryption-algorithm");
                this.validationAlgorithm = container.find("#key-validation-algorithm");

                container
                    .find("#generate-machine-key")
                    .on("click", this.generate);

                // Outputs
                this.textContainer = container.find("#machine-key-container");
                this.text = container.find("#machine-key-xml");
            }
        }

        /**
         * Generates a new machine key.
         * @param event - The event object.
         */
        private generate = (event: any): void => {

            event.preventDefault();

            const query = $.param({
                decryptionAlgorithm: this.decryptionAlgorithm.val(),
                validationAlgorithm: this.validationAlgorithm.val()
            });

            const uri = `${this.endpoint}?${query}`;

            $.get(uri).then((data) => {
                this.text.text(data.machineKeyXml);
                this.textContainer.removeClass("d-none");
            });
        }
    }
}
