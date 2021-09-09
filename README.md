# WEB API Sandbox

This is a sandbox for various things I wanted to try out with an executable application.
The application is currently just the default weather forecast app from the WEB API template. The application itself is not the point and does nothing it is just something I can plug different things into.

## Unobtrusive App.Metrics decorators using C#9 source generators

The objective was to eliminate most if not all boilerplate for adding performance metrics to an application. Requirements:
* Should not interfere with the application itself
  * Must handle and log all exceptions thrown from performance metrics code
* Must not tightly couple the application implementation code to the metrics library
  * Implemented as a decorator to an interface used in the application
  * Leverages `Srcutor.AspNetCore` library for DI integration
* Should be easy add new metrics
  * Uses conventions for naming and signatures
  * Should require only to add new files/types, not tough existing code
    * No switch-cases to maintain!
  * Has extension points for
    * Declaring variables and dependencies used by metrics code
    * Initializing variables used by metrics code
    * Finalizing metrics publishing
* Should be able to apply multiple metrics to one method
  * Generated code for different metrics should not conflict

See projects `Generators` and `MeasurementHelpers` for code. Attributes applied to `IServiceInterface` interface for testing purposes.

### Quick start guire
* The library that contains the source generators code must be `netstandard2.0`
* Must install the following NuGet packages to the generator library
  * Microsoft.CodeAnalysis.CSharp
  * Microsoft.CodeAnalysis.Analyzers
* The application that to hook into must reference the project in a special way. You need to edit the project file to set `OutputItemType="Analyzer" ReferenceOutputAssembly="false"`
```xml
    <ItemGroup>
      <ProjectReference Include="..\..\Generators\Generators.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    </ItemGroup>
```
* To debug the source generator code launch the debugger in `ISourceGenerator.Initialize`. Unfortunately debugging requires Visual Studio at the moment.
```c#
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
```
 

## Exception handling middleware
To be documented

## Feature flags
TODO

## Logging
To be documented