using System;
using System.Linq;

namespace KnowledgeGraphBuilder
{
    public static class StringExtensions
    {
        public static string RemoveAll(this string source, params string[] replacements)
        {
            var text = source;
            foreach (var r in replacements.Where(r => !String.IsNullOrWhiteSpace(r)))
            {
                text = text.Replace(r, "");
            }

            return text;
        }
    }
}