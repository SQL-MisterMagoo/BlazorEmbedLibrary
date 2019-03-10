
# BlazorEmbedLibrary

This is a component library that provides Blazor-style static file embedding for Razor Components/Blazor.

Chanegelog:

#### Version 0.1.0-beta-3
- Restructure source folders
- Add ability to handle multiple component assemblies in one go
- Add ability to block specific content files
-- Including in Blazor

#### Version 0.1.0-beta-2
- Add compatibility with Pre-Rendering

#### Version 0.1.0-beta-1
- Initial release 
- Enable support for embedded content files (CSS/JS) in Razor Components

Projects in this repo:

## BlazorEmbedLibrary

This is the component library that provides all the functionality.

It is a netstandard component library (i.e. a netstandard2.0 library) with one c# code file.

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

### Multiple Assemblies

From version 0.1.0-beta-3 onwards, you can now handle multiple component libraries in one hit using the Assemblies Parameter

```
<EmbeddedContent Assemblies="@Assemblies" />

@functions
{
List<System.Reflection.Assembly> Assemblies = new List<System.Reflection.Assembly>()
{
    typeof(BlazorComponentSample.Component1).Assembly,
    typeof(Blazored.Toast.BlazoredToasts).Assembly
};
}

```

### Block Files

From version 0.1.0-beta-3 onwards, you can now block individual files using the BlockCss Parameter

This example will load content from Blazored.Toast and Component1, but will block the *styles.css* from BlazorComponentSample.

```
<EmbeddedContent Assemblies="@Assemblies" BlockCssFiles="@BlockCss" />

@functions
{
List<string> BlockCss = new List<string>()
{
    "BlazorComponentSample,styles.css"
};

List<System.Reflection.Assembly> Assemblies = new List<System.Reflection.Assembly>()
{
    typeof(BlazorComponentSample.Component1).Assembly,
    typeof(Blazored.Toast.BlazoredToasts).Assembly
};
}

```

## Sample Projects

I have included Blazored.Toast and a simple custom component in the Razorcomponents and Blazor Apps


### BlazorComponentSample

An out-of-the-box sample Blazor component library with static files.

### BlazorEmbedContent - Runnable

A sample Blazor app to show how this library detects if Blazor has already linked the static files, and does not duplicate them.

Also shows how we can toggle CSS files on/off at runtime.

### RazorComponentsSample.Server - Runnable

A sample Razor Components App to show static files working.

Also shows how we can toggle CSS files on/off at runtime.
