// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Website>("Website")
       .WithHttpHealthCheck("/version");

var app = builder.Build();

app.Run();
