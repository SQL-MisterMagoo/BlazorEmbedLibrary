
# BlazorEmbedLibrary

This is a component library that provides Blazor-style static file embedding for Razor Components/Blazor.

Chanegelog:

#### Version 0.1.0-beta-4
- Add BlazorFileProvider : Static File Provider that serves embedded files from Blazor Libraries
- Add usage example to RazorComponentSample to serve files from BlazorComponentSample

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

It is a netstandard component library (i.e. a netstandard2.0 library).

## How to use this library

Add the nuget package BlazorEmbedLibrary

https://www.nuget.org/packages/BlazorEmbedLibrary/

#### Using the EmbeddedComponent to extract CSS/JS files

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

### Serve Static Files (images/data etc) using BlazorFileProvider

Available in 0.1.0-beta-4 is a new class that provides aspnetcore middleware to server embedded resources as static files in the pipeline.

To use this facility in a Razor Components project, add the nuget to your project and simply add the new BlazorFileProvider to your Configure method and pass it a list of Blazor Library assemblies.

```
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
  FileProvider = new BlazorFileProvider(
    new List<Assembly>() 
      { 
        typeof(BlazorComponentSample.Component1).Assembly 
      }
    )
});

```

_Note: Currently, there is a restriction that this provider will not allow duplicate file names. This is because CSS files added via the EmbeddedComponent appear in the root of the application, so any images requested by a relative URL will need to come from the root path as well, which means there is no way to distinguish between files with the same name. If two libraries have a file with the same name, the first one found will be used._
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
