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
