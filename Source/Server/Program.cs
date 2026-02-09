// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

var builder = WebApplication.CreateBuilder(args)
    .AddCratisArc(
        options =>
        {
            options.GeneratedApis.RoutePrefix = "api";
            options.GeneratedApis.IncludeCommandNameInRoute = false;
            options.GeneratedApis.SegmentsToSkipForRoute = 1;
        });

var app = builder.Build();

app.UseRouting();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseWebSockets();
app.MapControllers();
app.UseCratisArc();
app.MapFallbackToFile("/index.html");

await app.RunAsync();
