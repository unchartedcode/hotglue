using System;
using SassAndCoffee.JavaScript;

namespace HotGlue.Runtimes
{
    public class SassAndCoffeeRuntime : IJavaScriptRuntime
    {
        private string _code;
        
        public void LoadLibrary(string code)
        {
            //if (!String.IsNullOrWhiteSpace(_code)) throw new Exception("Library has already been loaded");
            _code = code;
        }

        public string Execute(string functionName, params object[] args)
        {
            // IEJavaScriptRuntime needs to be disposed
            // Eventually we should look at managing a "request" lifetime
            // so we can take advantage of running this a few times
            // Assuming we don't get COM exceptions...
            using (var javaScriptRuntime = new IEJavaScriptRuntime())
            {
                javaScriptRuntime.Initialize();
                javaScriptRuntime.LoadLibrary(_code);
                return javaScriptRuntime.ExecuteFunction<String>(functionName, args);
            }
        }
    }
}