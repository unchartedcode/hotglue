using System;
using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IPlugin
    {
        Dictionary<String, String> Variables { get; set; }
    }

    public interface IGenerateScriptReference : IPlugin
    {
        string GenerateHeader();
        string GenerateReference(SystemReference reference);
        string GenerateFooter();
    }
}