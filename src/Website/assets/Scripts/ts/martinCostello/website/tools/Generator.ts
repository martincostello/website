// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.website.tools {

    /**
     * Represents a generator.
     */
    export abstract class Generator {

        protected endpoint: string;

        /**
         * Initializes a new instance of the Generator class.
         */
        protected constructor() {
        }

        /**
         * Initializes the generator.
         */
        public abstract initialize(): void;
    }
}
