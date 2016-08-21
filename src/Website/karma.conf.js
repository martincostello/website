// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

module.exports = function (config) {
    config.set({

        autoWatch: false,
        concurrency: Infinity,

        browsers: ["PhantomJS"],
        frameworks: ["jasmine"],
        reporters: ["progress", "htmlDetailed"],

        files: [
            "wwwroot/lib/**/dist/*.js",
            "Assets/Scripts/js/martinCostello/martinCostello.js",
            "Assets/Scripts/js/martinCostello/website/website.js",
            "Assets/Scripts/js/martinCostello/website/debug.js",
            "Assets/Scripts/js/martinCostello/website/track.js",
            "Assets/Scripts/js/martinCostello/website/tools/tools.js",
            "Assets/Scripts/js/martinCostello/website/tools/guidGenerator.js",
            "Assets/Scripts/js/martinCostello/website/tools/hashGenerator.js",
            "Assets/Scripts/js/martinCostello/website/tools/machineKeyGenerator.js",
            "Assets/Scripts/**/*.spec.js"
        ],

        htmlDetailed: {
            splitResults: false
        },

        plugins: [
            "karma-chrome-launcher",
            "karma-html-detailed-reporter",
            "karma-jasmine",
            "karma-phantomjs-launcher"
        ]
    })
}
