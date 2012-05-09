using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HotGlue.Model;

namespace HotGlue
{
    public class GraphReferenceLocator : IReferenceLocator
    {
        private readonly HotGlueConfiguration _config;
        private readonly IEnumerable<IFindReference> _findReferences;

        public GraphReferenceLocator(HotGlueConfiguration config, IEnumerable<IFindReference> findReferences)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (!findReferences.Any())
            {
                throw new ArgumentException("No findReferences found, nothing to parse");
            }
            _config = config;
            _findReferences = findReferences;
        }

        public IEnumerable<Reference> Load(string rootPath, Reference reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            // Ensure we have the root path separate from the reference
            var rootIndex = reference.Path.IndexOf(rootPath, StringComparison.OrdinalIgnoreCase);
            var relativePath = reference.Path;
            if (rootIndex >= 0)
            {
                relativePath = relativePath.Substring(rootIndex+rootPath.Length);
                if (relativePath.Length > 1 && relativePath.StartsWith("/") || relativePath.StartsWith("\\"))
                {
                    relativePath = relativePath.Substring(1);
                }
            }

            var results = Parse(rootPath, relativePath, _config.ScriptSharedFolder, reference.Name);

            if (!results.Any())
            {
                yield return reference;
            }

            CheckForCircularReferences(results);

            // Return the files in the correct order
            var processed = new List<Reference>();
            var loops = 0;
            var maxLoops = results.Count;
            while (results.Any())
            {
                if (loops++ > maxLoops)
                {
                    throw new StackOverflowException();
                }

                var noDependencies = results.Where(x => x.Value.Count(v => !processed.Contains(v)) == 0)
                                            .OrderBy(x => x.Key).ToList();
                if (!noDependencies.Any())
                {
                    throw new Exception("Unable to add order, there aren't any files that don't have a dependency");
                }

                foreach (var noDependency in noDependencies)
                {
                    processed.Add(noDependency.Key);
                    results.Remove(noDependency.Key);
                    yield return noDependency.Key;
                }
            }
        }

        private void CheckForCircularReferences(Dictionary<Reference, IList<Reference>> references)
        {
            // Check for circular reference, if there are any, loading order won't work.
            Action<Reference, KeyValuePair<Reference, IList<Reference>>> compareChildren = null;
            compareChildren = (reference, list) =>
                {
                    foreach (var childReference in list.Value)
                    {
                        if (childReference.Equals(reference))
                        {
                            throw new Exception(String.Format("Circular reference detected between file '{0}' and '{1}'", reference.GetPath(), list.Key.GetPath()));
                        }
                        if (references.ContainsKey(childReference))
                        {
                            compareChildren(reference, references.Single(x => x.Key.Equals(childReference)));
                        }
                    }
                };

            foreach (var root in references.Where(root => root.Value.Any()))
            {
                compareChildren(root.Key, root);
            }
        }

        private Dictionary<Reference, IList<Reference>> Parse(String rootPath, String relativePath, String sharedFolder, String fileName)
        {
            String currentPath = Path.Combine(rootPath, relativePath);
            String sharedPath = null;
            if (!String.IsNullOrWhiteSpace(sharedFolder))
            {
                sharedPath = Path.Combine(rootPath, sharedFolder);
            }
            var references = new Dictionary<Reference, IList<Reference>>();
            Parse(currentPath, sharedPath, new Reference() { Name =  fileName }, references);
            return references;
        }

        // recursive function
        private void Parse(String currentPath, String sharedPath, Reference parentReference, Dictionary<Reference, IList<Reference>> references)
        {
            Reference reference = null;
            var currentFile = Path.Combine(currentPath, parentReference.Name);
            if (File.Exists(currentFile))
            {
                reference = new Reference() { Path = currentPath, Name = parentReference.Name, Module = parentReference.Module };
            }
            else if (!String.IsNullOrWhiteSpace(sharedPath))
            {
                var sharedFile = Path.Combine(sharedPath, parentReference.Name);
                if (File.Exists(sharedFile))
                {
                    currentPath = sharedPath; // Once in shared, only look at shared for other models.
                    reference = new Reference() { Path = sharedPath, Name = parentReference.Name, Module = parentReference.Module };
                }
            }

            if (reference == null)
            {
                throw new FileNotFoundException(String.Format("Unable to find the file: '{0}' in either the current path: '{1}', or the shared path: '{2}'.", parentReference.Name, currentPath, sharedPath));
            }

            parentReference.Path = reference.Path;
            var newReferences = HasReferences(reference, references);
            foreach (var fileReference in newReferences)
            {
                Parse(currentPath, sharedPath, fileReference, references);
            }
        }

        private IList<Reference> HasReferences(Reference reference, Dictionary<Reference, IList<Reference>> references)
        {
            if (references.ContainsKey(reference))
            {
                var existing = references.Keys.Single(x => x.Equals(reference));
                if (existing.Module != reference.Module)
                {
                    throw new Exception(String.Format("A different require reference was found for the file: '{0}'. You can only have //=requires or var variable = require('') for all references to the same file.", reference.GetPath()));
                }
                return new List<Reference>(); // already parsed file
            }

            references.Add(reference, new List<Reference>());

            var text = File.ReadAllText(reference.GetPath());
            var currentReferences = new List<Reference>();
            foreach (var findReference in _findReferences)
            {
                currentReferences.AddRange(findReference.Parse(text));
            }

            var newReferences = new List<Reference>();
            var list = references[reference];
            foreach (var currentReference in currentReferences)
            {
                if (!list.Contains(currentReference))
                {
                    newReferences.Add(currentReference);
                    references[reference].Add(currentReference);
                }
            }

            return newReferences;
        }
    }
}