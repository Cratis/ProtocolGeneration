# Spec

We are building a code generator that can take arbitrary code and generate gRPC contracts in
the form of C# interfaces compatible with Protobuf.net. These should then be runtime exposed
automatically and routed automatically from the exposed gRPC / Protobuf to the correct code.

## Backend

Build an imaginary backend for an eCommerce. Not a full one, just to test a couple of scenarios.
It should expose only commands and queries leveraging the model bound approach of Cratis Arc.
Its important that we capture both regular queries and observable queries using ISubject<>.
Also important that we get a variety of queries for both with single items and IEnumerable<> of items.

Read more here:

https://www.cratis.io/docs/Arc/index.html
https://www.cratis.io/docs/Arc/backend/queries/model-bound/index.html

The implementation of commands can typically just print stuff. This is not important.
Queries can return static data, except for those queries that are observable. It should
then just on a regular cadence trigger an update to the subject.

Keep a structure for different modules within the backend with specific namespaces and house
different commands and queries.

There should be a new attribute called `[BelongsTo(string)]` this should be used to tell which
service the command and / or query belongs to.

Value types should be created using Concepts, read more here: https://www.cratis.io/docs/Fundamentals/csharp/concepts.html.

Include both the use of OneOf<> from the OneOf library and DateTimeOffset in one or more command and query.

## Code generation

Code generation should be a CLI tool that loads an assembly and recognizes types such as Commands, Queries and Observable Queries.
From this it should generate C# interfaces to a specified target folder.

It should gather all commands and queries and from convention combine all commands and queries within a namespace
into gRPC services named according to the `[BelongsTo]`attribute is saying.

If the command or query does not have a `[BelongsTo]` attribute - just ignore it and print a warning instead.

The tool needs to have the option of skipping a number namespace segments.
The tool needs to have the option of specifying base namespace for the generated interfaces.

It should create a folder for each namespace that matches the namespace from the source type.
If there is a mismatch in namespace between the things being generated into the target service name based on `[BelongsTo]`,
we should stop generating and consider this an error and print out the problem.

The generator should take inspiration from how we do it with the ProxyGenerator in Cratis Arc for TypeScript.
See how things are organized and also how we do assembly loading and making it work in general.
You'll find it here: https://github.com/Cratis/Arc/tree/main/Source/DotNET/Tools/ProxyGenerator.

It needs to be able to unwrap ConceptAs<> into the primitive type when generating proto contracts.
Support the OneOf and SerializableDateTimeOffset as well. If something returns IOneOf<> from the OneOf library,

Also add a MSBuild project that automatically runs the code generator with properties, see how the ProxyGenerator does this here:
https://github.com/Cratis/Arc/tree/main/Source/DotNET/Tools/ProxyGenerator.Build

## Server

Server should automatically expose all service contracts marked with the `[Service]` attribute.
It should automatically at runtime expose all the services and create an automatic router that
routes from the gRPC exposed routes and transforms to the correct construct and forwards the calls.

It should leverage the ICommandPipeline (https://github.com/Cratis/Arc/blob/main/Source/DotNET/Arc.Core/Commands/ICommandPipeline.cs)
for calling commands. And then the IQueryPipeline (https://github.com/Cratis/Arc/blob/main/Source/DotNET/Arc.Core/Queries/IQueryPipeline.cs)
for doing queries.

Serialization needs to be handled properly. This means we should be able to expose types as we'd like,
but be compatible with what the code generator generates and be able to convert / deserialize to the target type.
ConceptAs<> has something called ConceptFactory that can create an instance of a ConceptAs<> from a primitive.
Located in the Cratis.Concepts namespace.

Goal is to not have to write any code other than a command or a query and it just gets exposed automatically.

Expose gRPC Reflection for the server as well.

## Client

The client should be able to connect using standard Protobuf.net.
Just add some keyboard input to perform different things so that we can test out the client talking to the server.

## General

For the Code Generation we want a Specs project once everything has settled.
Keep the code quality high - we're betting on this experiment to become how we do things in production.