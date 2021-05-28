// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.website.tools {

    /**
     * Represents a GUID generator.
     */
    export class GuidGenerator extends Generator {

        private format: JQuery<Element>;
        private text: JQuery<Element>;
        private uppercase: JQuery<Element>;

        /**
         * Initializes a new instance of the GuidGenerator class.
         */
        public constructor() {
            super();
        }

        /**
         * Initializes the generator.
         */
        public initialize(): void {

            this.endpoint = $("link[name='api-guid']").attr("href");

            if (this.endpoint) {

                const container = $("#generate-guid-container");

                // Inputs
                this.format = container.find("#guid-format");
                this.uppercase = container.find("#guid-uppercase");

                container
                    .find("#generate-guid")
                    .on("click", this.generateGuid);

                // Outputs
                this.text = container.find("#text-guid");
            }
        }

        /**
         * Handles the button for generating a new GUID being clicked.
         * @param event - The event object.
         */
        private generateGuid = (event: any): void => {

            event.preventDefault();

            const query = $.param({
                format: this.format.val(),
                uppercase: this.uppercase.is(":checked")
            });

            const uri = `${this.endpoint}?${query}`;

            $.get(uri).then((data) => {
                this.text.val(data.guid);
                this.text.attr("value", data.guid);
            });
        }
    }
}
