/// <binding ProjectOpened='watch' />

// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

const browserify = require('browserify');
const buffer = require('vinyl-buffer');
const concat = require('gulp-concat');
const csslint = require('gulp-csslint');
const cssmin = require('gulp-cssmin');
const eslint = require('gulp-eslint');
const gulp = require('gulp');
const jest = require('gulp-jest').default;
const prettier = require('gulp-prettier');
const source = require('vinyl-source-stream');
const sourcemaps = require('gulp-sourcemaps');
const tsify = require('tsify');
const uglify = require('gulp-uglify');

const paths = {
    css: './assets/styles/**/*.css',
    ts: './assets/scripts/**/*.ts'
};

gulp.task('format:ts', function () {
    return gulp.src(paths.ts)
        .pipe(prettier())
        .pipe(gulp.dest(file => file.base));
});

gulp.task('lint:css', function () {
    return gulp.src(paths.css)
        .pipe(csslint())
        .pipe(csslint.formatter())
        .pipe(csslint.formatter('fail'));
});

gulp.task('lint:ts', function () {
    return gulp.src(paths.ts)
      .pipe(eslint({
        formatter: 'visualstudio'
      }))
      .pipe(eslint.format())
      .pipe(eslint.failAfterError());
});

gulp.task('build:css', function () {
    return gulp.src(paths.css, { base: '.' })
        .pipe(sourcemaps.init())
        .pipe(concat('wwwroot/assets/css/site.css'))
        .pipe(cssmin())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('.'));
});

gulp.task('build:ts', function () {
    return browserify({
      basedir: '.',
      debug: true,
      entries: ['assets/scripts/main.ts'],
      cache: {},
      packageCache: {}
    })
      .plugin(tsify)
      .transform('babelify', {
        presets: ['@babel/preset-env'],
        extensions: ['.ts']
      })
      .bundle()
      .pipe(source('site.js'))
      .pipe(buffer())
      .pipe(sourcemaps.init({ loadMaps: true }))
      .pipe(uglify())
      .pipe(sourcemaps.write('./'))
      .pipe(gulp.dest('wwwroot/assets/js'));
});

gulp.task('test:ts', function () {
    return gulp.src('assets')
        .pipe(jest({
            collectCoverage: true
        }));
});

gulp.task('default:css', gulp.series('lint:css', 'build:css'));
gulp.task('default:ts', gulp.series('format:ts', 'lint:ts', 'build:ts', 'test:ts'));

gulp.task('default', gulp.parallel('default:css', 'default:ts'));

gulp.task('watch', function () {
    gulp.watch(paths.css, gulp.series('build:css'));
    gulp.watch(paths.ts, gulp.series('lint:ts', 'build:ts'));
});
