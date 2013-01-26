﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue
{
    public class Package : IPackage
    {
        private readonly string _relativeRoot;
        private readonly IEnumerable<ICompile> _compilers;
        private readonly IGenerateScriptReference _generateScriptReference;
        private static string _scriptPath;
        private static IFileCache _cache;

        public Package(string relativeRoot, IEnumerable<ICompile> compilers, IGenerateScriptReference generateScriptReference)
        {
            _relativeRoot = relativeRoot;
            _compilers = compilers;
            _generateScriptReference = generateScriptReference;
        }

        public static Package Build(HotGlueConfiguration configuration, string root, IFileCache cache)
        {
            IGenerateScriptReference generateScriptReference;
            if (configuration == null || configuration.GenerateScript == null)
            {
                generateScriptReference = new HTMLGenerateScriptReference();
            }
            else
            {
                generateScriptReference = (IGenerateScriptReference)Activator.CreateInstance(Type.GetType(configuration.GenerateScript.Type));
            }

            IList<ICompile> compilers;
            if (configuration == null || configuration.Compilers == null || configuration.Compilers.Length == 0)
            {
                compilers = new ICompile[]
                    {
                        //new CoffeeScriptCompiler()
                    };
            }
            else
            {
                compilers = configuration.Compilers
                    .Where(c => string.IsNullOrWhiteSpace(c.Mode) || c.Mode.Equals(configuration.Debug ? "debug" : "release", StringComparison.OrdinalIgnoreCase))
                    .Select(compiler => (ICompile)Activator.CreateInstance(Type.GetType(compiler.Type)))
                    .ToList();
            }

            _scriptPath = configuration.ScriptPath;
            _cache = cache;

            var package = new Package(root, compilers, generateScriptReference);
            return package;
        }

        public string Compile(IEnumerable<SystemReference> references)
        {
            if (references == null) return "";

            var sw = new StringBuilder();
            var modules = false;

            foreach(var reference in references)
            {
                switch(reference.Type)
                {
                    case Reference.TypeEnum.App:
                        if (modules) sw.AppendLine(CompileStitch());
                        sw.AppendLine(CompileDependency(reference));
                        break;
                    case Reference.TypeEnum.Library:
                    case Reference.TypeEnum.Dependency:
                        sw.AppendLine(CompileDependency(reference));
                        break;
                    case Reference.TypeEnum.Module:
                        modules = true;
                        sw.AppendLine(CompileModule(reference));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return sw.ToString();
        }

        public string CompileDependency<T>(T reference) where T : Reference
        {
            var cacheKey = reference.Path + "/" + reference.Name;
            DateTime lastWriteTimeUtc = (reference is SystemReference)
                ? new FileInfo((reference as SystemReference).FullPath).LastWriteTimeUtc
                : new DateTime(); // Cached forever

            if (_cache != null)
            {
                dynamic cached = _cache.Get(cacheKey);
                if (cached != null && cached.LastWriteTimeUtc.Equals(lastWriteTimeUtc))
                {
                    return cached.Content;
                }
            }

            if (reference.Content == null && reference is SystemReference)
            {
                var systemReference = reference as SystemReference;
                reference.Content = File.ReadAllText(systemReference.FullPath);
            }

            foreach(var compiler in _compilers)
            {
                if (compiler.Handles(reference.Extension))
                {
                    compiler.Compile(ref reference);
                }
            }

            if (_cache != null)
            {
                _cache.Set(cacheKey, new { LastWriteTimeUtc = lastWriteTimeUtc, Content = reference.Content });
            }

            return reference.Content;
        }
        
        public string CompileModule(SystemReference reference)
        {
            var itemName = "";
            if (string.IsNullOrEmpty(itemName))
            {
                if (string.IsNullOrEmpty((reference.Name)))
                {
                    throw new InvalidOperationException("Either name or reference.Name is required here.");
                }
                itemName = reference.Name.ToLower();
                if (!string.IsNullOrEmpty(reference.Path))
                {
                    itemName = reference.Path.ToLower() + "/" + reference.Name.ToLower().Replace("\\", "/");
                }
                itemName = itemName.Replace("\\", "/");
            }

            var sb = new StringBuilder();
            sb.Append(@"if(typeof(__hotglue_assets)==='undefined'){__hotglue_assets={};}__hotglue_assets['" + itemName + @"'] = { keys: [ '" + String.Join(",", reference.ReferenceNames).Replace(",","','") + "' ], item: function(exports, require, module) {");
            sb.Append(CompileDependency(reference));
            sb.Append("}};");

            return sb.ToString();
        }

        public string CompileStitch()
        {
            return CompileDependency(new Reference { Extension = ".js", Content = Properties.Resources.stitch });
        }

        public string References(IEnumerable<SystemReference> references)
        {
            if (references == null) return "";

            var sw = new StringBuilder();
            var modules = false;

            foreach (var reference in references)
            {
                switch (reference.Type)
                {
                    case Reference.TypeEnum.App:
                        if (modules)
                        {
                            var systemReference = new SystemReference(new DirectoryInfo(_relativeRoot), new FileInfo(_relativeRoot + _scriptPath + "/get.js-require"), "get.js-require")
                                                  {
                                                      Type = Reference.TypeEnum.Dependency,
                                                      Wait = true
                                                  };
                            sw.AppendLine(_generateScriptReference.GenerateReference(systemReference));
                        }
                        sw.AppendLine(_generateScriptReference.GenerateReference(reference));
                        break;
                    case Reference.TypeEnum.Library:
                    case Reference.TypeEnum.Dependency:
                        sw.AppendLine(_generateScriptReference.GenerateReference(reference));
                        break;
                    case Reference.TypeEnum.Module:
                        modules = true;
                        var clone = reference.Clone("-module");
                        sw.AppendLine(_generateScriptReference.GenerateReference(clone));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return sw.ToString();
        }
    }
}
