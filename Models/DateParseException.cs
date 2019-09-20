using System;

namespace KnowledgeGraphBuilder
{
    public class DateParseException : Exception
    {
        public DateParseException(string s) : base(s)
        {
        }
    }
}