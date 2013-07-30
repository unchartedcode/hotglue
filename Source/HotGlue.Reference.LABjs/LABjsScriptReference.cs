using System;
using System.Collections.Generic;
using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class LABjsScriptReference : IGenerateScriptReference
    {
        public Dictionary<string, string> Variables { get; set; }

        private bool GenerateHeaderAndFooter
        {
            get
            {
                if (Variables == null)
                {
                    return true;
                }

                if (!Variables.ContainsKey("GenerateHeaderAndFooter"))
                {
                    return true;
                }

                return Convert.ToBoolean(Variables["GenerateHeaderAndFooter"]);
            }
        }

        public string GenerateHeader()
        {
            return GenerateHeaderAndFooter ? @"<script>
$LAB
" : "";
        }

        public string GenerateReference(SystemReference reference)
        {
            var relativePath = reference.RelativePath(true);
            var wait = reference.Wait ? ".wait()" : "";

            return reference.Name.EndsWith("-module")
                       ? string.Format(".script(\"/hotglue.axd{0}&name={2}\"){1}", relativePath, wait, string.Join("&name=", reference.ReferenceNames))
                       : string.Format(".script(\"/hotglue.axd{0}\"){1}", relativePath, wait);
        }

        public string GenerateFooter()
        {
            return GenerateHeaderAndFooter ? @";
</script>
" : "";
        }
    }
}