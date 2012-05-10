﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HotGlue.Compilers
{
    public class JQueryTemplateCompiler : ICompile
    {
        public List<string> Extensions { get; private set; }

        public JQueryTemplateCompiler()
        {
            Extensions = new List<string>(new[] { ".tmpl" });
        }

        public bool Handles(string Extension)
        {
            return Extensions.Where(e => e == Extension).Any();
        }

        public string Compile(string Data)
        {
            var content = Data.Replace("\r\n", "").Replace("\n", "").Replace("\"", "'");
            return @"
var template = jQuery.template(""" + content + @""");
module.exports = (function(data){ return jQuery.tmpl(template, data); });";
        }
    }
}
