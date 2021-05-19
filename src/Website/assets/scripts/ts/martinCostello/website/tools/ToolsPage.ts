// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

declare var ClipboardJS: any;

namespace martinCostello.website.tools {

    /**
     * Represents the class for the tools page.
     */
    export class ToolsPage {

        private guid: GuidGenerator;
        private hash: HashGenerator;
        private key: MachineKeyGenerator;

        /**
         * Initializes a new instance of the ToolsPage class.
         */
        public constructor() {
            this.guid = new GuidGenerator();
            this.hash = new HashGenerator();
            this.key = new MachineKeyGenerator();
        }

        /**
         * Initializes the page.
         */
        public initialize(): void {

            this.guid.initialize();
            this.hash.initialize();
            this.key.initialize();

            if ("ClipboardJS" in window && ClipboardJS !== undefined) {

                const selector = ".copy-button";
                const copyButton = $(selector);

                const clipboard = new ClipboardJS(selector);

                copyButton.on("click", (event) => {
                    event.preventDefault();
                });
            }
        }
    }
}

(() => {
    $(document).ready((): void => {
        const page = new martinCostello.website.tools.ToolsPage();
        page.initialize();
    });
})();
