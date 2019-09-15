using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorComponentSample
{
    public class ExampleJsInterop
    {
        public static ValueTask<string> Prompt(IJSRuntime jSRuntime, string message)
        {
            // Implemented in exampleJsInterop.js
            return jSRuntime.InvokeAsync<string>(
                "exampleJsFunctions.showPrompt",
                message);
        }
    }
}
