﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebsite();

var app = builder.Build();

app.UseWebsite();

app.Run();
