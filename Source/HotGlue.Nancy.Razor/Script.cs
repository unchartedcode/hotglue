﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Demos.Nancy;
using HotGlue.Model;
using Nancy;
using Nancy.ViewEngines.Razor;

namespace HotGlue
{
    public static class Script
    {
        private static LoadedConfiguration _configuration;
        private static bool _debug;
        private static IReferenceLocator _locator;

        static Script()
        {
            _debug = StaticConfiguration.IsRunningDebug;
            var config = HotGlueConfiguration.Load(_debug);
            _configuration = LoadedConfiguration.Load(config);
            _locator = new GraphReferenceLocator(_configuration);
        }

        public static IHtmlString Reference(string name)
        {
            var root = HotGlueNancyStartup.Root;
            return new NonEncodedHtmlString(ScriptHelper.Reference(_configuration, _locator, root, name, _debug));
        }
    }
}
