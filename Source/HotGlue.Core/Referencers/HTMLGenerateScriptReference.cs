using System;
using System.Collections.Generic;
using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class HTMLGenerateScriptReference : IGenerateScriptReference
    {
        public Dictionary<string, string> Variables { get; set; }

        public string GenerateHeader()
        {
            return "";
        }

        public string GenerateReference(SystemReference reference)
        {
            var relativePath = reference.RelativePath(true);

            return reference.Name.EndsWith("-module")
                       ? string.Format("<script src=\"/hotglue.axd{0}&name={1}\"></script>", relativePath, string.Join("&name=", reference.ReferenceNames))
                       : string.Format("<script src=\"/hotglue.axd{0}\"></script>", relativePath)
                       + Environment.NewLine;
        }

        public string GenerateFooter()
        {
            return "";
        }
    }
}