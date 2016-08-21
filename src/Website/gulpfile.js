// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/// <binding AfterBuild='build' Clean='clean' />
"use strict";

var gulp = require("gulp"),
    del = require("del"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    csslint = require("gulp-csslint"),
    jasmine = require("gulp-jasmine"),
    jshint = require("gulp-jshint"),
    karmaServer = require("karma").Server,
    less = require("gulp-less"),
    lesshint = require("gulp-lesshint"),
    rename = require("gulp-rename"),
    sass = require("gulp-sass"),
    sassLint = require("gulp-sass-lint");

var webroot = "./wwwroot/Assets/";
var assets = "./Assets/";
var scripts = assets + "Scripts/";
var styles = assets + "Styles/";

var paths = {
    jsSrc: scripts + "js/",
    js: scripts + "js/**/*.js",
    jsDest: webroot + "js",
    minJs: webroot + "js/**/*.min.js",
    minJsDest: webroot + "js/site.min.js",
    testsJs: scripts + "**/*.spec.js",
    css: styles + "css/**/*.css",
    minCssDest: webroot + "css/**/site.min.css",
    concatJsDest: webroot + "js/site.js",
    concatCssDest: webroot + "css/site.css",
    less: styles + "/less/site.less",
    lessDest: webroot + "css",
    sass: styles + "/sass/site.scss",
    sassDest: webroot + "css",
    cssClean: webroot + "css/**/*.css",
    jsClean: webroot + "js/**/*.js"
};

gulp.task("clean:js", function (cb) {
    return del([paths.jsClean]);
});

gulp.task("clean:css", function (cb) {
    return del([paths.cssClean]);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("css:less", function () {
    return gulp.src(paths.less)
      .pipe(less())
      .pipe(rename("less.css"))
      .pipe(gulp.dest(paths.lessDest));
});

gulp.task("css:sass", function () {
    return gulp.src(paths.sass)
      .pipe(sass().on("error", sass.logError))
      .pipe(rename("sass.css"))
      .pipe(gulp.dest(paths.sassDest));
});

gulp.task("css", ["css:less", "css:sass"]);

gulp.task("lint:css", function () {
    return gulp.src(styles)
      .pipe(csslint())
      .pipe(csslint.reporter())
      .pipe(csslint.reporter("fail"));
});

gulp.task("lint:js", function () {
    return gulp.src(paths.js)
      .pipe(jshint())
      .pipe(jshint.reporter("default"))
      .pipe(jshint.reporter("fail"));
});

gulp.task("lint:less", function () {
    return gulp.src(paths.less)
        .pipe(lesshint())
        .pipe(lesshint.reporter());
});

gulp.task("lint:sass", function () {
    gulp.src(paths.sass)
      .pipe(sassLint())
      .pipe(sassLint.format())
      .pipe(sassLint.failOnError());
});

gulp.task("lint", ["lint:js", "lint:less", "lint:sass", "lint:css"]);

gulp.task("min:js", function () {

    var jsPaths = [
        paths.jsSrc + "analytics.js",
        paths.jsSrc + "clipboard.js",
        paths.jsSrc + "lazyload.js",
        paths.jsSrc + "martinCostello/martinCostello.js",
        paths.jsSrc + "martinCostello/website/website.js",
        paths.jsSrc + "martinCostello/website/debug.js",
        paths.jsSrc + "martinCostello/website/track.js",
        paths.jsSrc + "martinCostello/website/tools/tools.js",
        paths.jsSrc + "martinCostello/website/tools/guidGenerator.js",
        paths.jsSrc + "martinCostello/website/tools/hashGenerator.js",
        paths.jsSrc + "martinCostello/website/tools/machineKeyGenerator.js",
        "!" + paths.jsSrc + "**/*.spec.js",
        "!" + paths.minJs,
        "!" + paths.concatJsDest
    ];

    return gulp.src(jsPaths)
        .pipe(concat(paths.concatJsDest))
        .pipe(gulp.dest("."))
        .pipe(uglify())
        .pipe(rename(paths.minJsDest))
        .pipe(gulp.dest("."));
});

gulp.task("min:css", ["css"], function () {
    return gulp.src([
            paths.lessDest + "/less.css",
            paths.sassDest + "/sass.css",
            paths.css,
            "!" + paths.minCss,
            "!" + paths.concatCssDest])
        .pipe(concat(paths.concatCssDest))
        .pipe(gulp.dest("."))
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:css"]);

gulp.task("test:js:karma", function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        singleRun: true
    }, done).start();
});

gulp.task("test:js:chrome", function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        browsers: ["Chrome"]
    }, done).start();
});

gulp.task("test:js", ["test:js:karma"]);
gulp.task("test", ["test:js"]);

gulp.task("build", ["lint", "min"]);
gulp.task("publish", ["build", "test"]);
