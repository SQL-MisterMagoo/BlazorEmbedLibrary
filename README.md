
# BlazorEmbedLibrary

This is a component library that provides Blazor-style static file embedding for Razor Components.

Projects in this repo:

## BlazorEmbedLibrary

This is the component library that provides all the functionality.

It is a netstandard component library (i.e. a netstandard2.0 library) with one c# code file.

I have included two sample static files, one css that makes the body background cyan, and one JS file that you can test from the dev tools console `testjs.testfunc()` - shows an alert.

### BlazorComponentSample

An out-of-the-box sample Blazor component library with static files.

### BlazorEmbedContent - Runnable

A sample Blazor app to show how this library detects if Blazor has already linked the static files, and does not duplicate them.

### RazorComponentsSample.App

A sample Razor Components app to show static files working.

### RazorComponentsSample.Server - Runnable

A sample Razor Components server to show static files working.

## How to use this library

Add the nuget package BlazorEmbedLibrary

https://www.nuget.org/packages/BlazorEmbedLibrary/

Add a *Using* and an *addTagHelper* to the __ViewImports file

```
@using BlazorEmbedLibrary
@addTagHelper *, BlazorEmbedLibrary
```

Then add the component to whichever page you want e.g. MainLayout, Index.cshtml - wherever makes sense for your project/needs.

```
<EmbeddedContent BaseType="@(typeof(Component1))" />
```

Note, by default the EmbeddedContent component has Debug turned off - if you enable it by setting Debug=true, it outputs the list of embedded resources.

This will read any CSS or Js files, which are embedded resources, from the Component1 library and add them to the `head` of the document.

## Examples

I have included Blazored.LocalStorage and Blazored.Toast examples in the Razorcomponents App

