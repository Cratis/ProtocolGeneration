// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Server;

var builder = WebApplication.CreateBuilder(args)
    .AddCratisArc(
        options =>
        {
            options.GeneratedApis.RoutePrefix = "api";
            options.GeneratedApis.IncludeCommandNameInRoute = false;
            options.GeneratedApis.SegmentsToSkipForRoute = 1;
        });

// Add gRPC services
builder.Services.AddGrpc();
builder.Services.AddAutoDiscoveredGrpcServices();

// Add gRPC reflection
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.UseRouting();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseWebSockets();
app.MapControllers();
app.UseCratisArc();

// Map gRPC services
app.MapAutoDiscoveredGrpcServices();

// Map gRPC reflection (for tools like grpcurl)
app.MapGrpcReflectionService();

app.MapFallbackToFile("/index.html");

await app.RunAsync();
