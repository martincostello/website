﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

$(window).on("load", function () {
    setTimeout(function () {
        $("img.lazy").lazyload();
    }, 500);
});
