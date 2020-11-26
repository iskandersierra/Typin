﻿namespace TypinExamples.Infrastructure.WebWorkers.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    public class ScriptLoader
    {
        public const string ModuleName = "BlazorWebWorker";
        private const string JS_FILE = "BlazorWebWorker.js";

        private static readonly IReadOnlyDictionary<string, string> escapeScriptTextReplacements =
            new Dictionary<string, string> { { @"\", @"\\" }, { "\r", @"\r" }, { "\n", @"\n" }, { "'", @"\'" }, { "\"", @"\""" } };

        private readonly IJSRuntime jsRuntime;

        public ScriptLoader(IJSRuntime jSRuntime)
        {
            jsRuntime = jSRuntime;
        }

        public async Task InitScript()
        {
            if (await IsLoaded())
                return;

            string scriptContent;
            Assembly assembly = typeof(ScriptLoader).Assembly;
            string assemblyName = assembly.GetName().Name ?? throw new InvalidOperationException($"Unable to initialize {JS_FILE}");

            using (Stream stream = assembly.GetManifestResourceStream($"{assemblyName}.{JS_FILE}") ?? throw new InvalidOperationException($"Unable to get {JS_FILE}"))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                    scriptContent = await streamReader.ReadToEndAsync();
            }

            await ExecuteRawScriptAsync(scriptContent);

            int loaderLoopBreaker = 0;
            while (!await IsLoaded())
            {
                loaderLoopBreaker++;
                await Task.Delay(100);

                // Fail after 3s not to block and hide any other possible error
                if (loaderLoopBreaker > 25)
                    throw new InvalidOperationException($"Unable to initialize {JS_FILE}");
            }
        }

        private async Task<bool> IsLoaded()
        {
            return await jsRuntime.InvokeAsync<bool>("window.hasOwnProperty", ModuleName);
        }

        private async Task ExecuteRawScriptAsync(string scriptContent)
        {
            scriptContent = escapeScriptTextReplacements.Aggregate(scriptContent, (r, pair) => r.Replace(pair.Key, pair.Value));

            string blob = $"URL.createObjectURL(new Blob([\"{scriptContent}\"],{{ \"type\": \"text/javascript\"}}))";
            string bootStrapScript = $"(function(){{var d = document; var s = d.createElement('script'); s.async=false; s.src={blob}; d.head.appendChild(s); d.head.removeChild(s);}})();";

            await jsRuntime.InvokeVoidAsync("eval", bootStrapScript);
        }
    }
}
