using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HotGlue.Model;

namespace HotGlue
{
    /// <summary>
    /// Finds references in the format of
    /// 
    ///    var variable = require('test.js');   OR
    ///    variable = require('test.js')        OR
    ///    variable = require 'test.js'
    /// </summary>
    public class RequireReference : IFindReference
    {
        private static readonly Regex ReferenceVariableRegex = new Regex(
            @"^\s*(?!//|#|/\*)(?:var\s+)?(?<variable>\S+)?\s*((=|:){1}\s*)?require(\(|\s)(""|')(?<path>.+?)(""|')\)?\S*;?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture
            );

        public IEnumerable<RelativeReference> Parse(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) yield break;

            var matches = ReferenceVariableRegex.Matches(fileText)
                .Cast<Match>()
                .Where(m => !String.IsNullOrWhiteSpace(m.Groups["path"].Value))
                .Select(MatchToRelativeReference);

            foreach (var match in matches)
            {
                yield return match;
            }
        }

        private RelativeReference MatchToRelativeReference(Match match)
        {
            var group = match.Groups["path"];
            return new RelativeReference(group.Value, group.Index) {Type = Reference.TypeEnum.Module};
        }
    }
}