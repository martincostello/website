// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(function () {
    $(document).ready(function () {
        if ("Clipboard" in window) {
            var selector = ".copy-button";
            var copyButton = $(selector);

            new Clipboard(selector);

            copyButton.click(function (event) {
                event.preventDefault();
            });
        }
    });
})();
