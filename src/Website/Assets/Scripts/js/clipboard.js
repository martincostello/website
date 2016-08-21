// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(function () {
    $(document).ready(function () {

        var swfPath = $("link[rel='clipboard-swf']").attr("href");

        if (swfPath) {

            var copyButton = $(".copy-button");
            var clipboardInitialized = false;

            ZeroClipboard.config({
                swfPath: swfPath
            });

            new ZeroClipboard(copyButton);

            copyButton.click(function (event) {
                event.preventDefault();
            });
        }
    });
})();
