using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericParsing;

namespace KnowledgeGraphBuilder
{
    public class SeriesReader
    {
        public static IEnumerable<Series> ReadHeaders(string file, string tsvFolder)
        {
            using (var parser = new GenericParser(file))
            {
                parser.ColumnDelimiter = '\t';
                parser.FirstRowHasHeader = true;
                parser.TrimResults = true;
                parser.MaxBufferSize = parser.MaxBufferSize * 10;

                while (parser.Read())
                {
                    var fileName = parser["TSV files"];
                    yield return new Series
                    {
                        File = fileName,
                        SeriesRef = parser["Series"],
                        Title = parser["Title"], 
                        Entries = ReadEntries(Path.Combine(tsvFolder, fileName)).ToArray()
                    };
                }
            }
        }
        
        public static IEnumerable<SeriesEntry> ReadEntries(string file)
        {
            using (var parser = new GenericParser(file))
            {
                parser.ColumnDelimiter = '\t';
                parser.FirstRowHasHeader = true;
                parser.TrimResults = true;
                parser.MaxBufferSize = parser.MaxBufferSize * 10;

                while (parser.Read())
                {
                    if (!String.IsNullOrWhiteSpace(parser["Level"]))
                    {
                        yield return new SeriesEntry
                        {
                            Id = parser["ID"],
                            Description = parser["Description"],
                            Reference = parser["Reference"],
                            Title = parser["Title"],
                            RelatedMaterial = parser["Related Material"],
                            SeperatedMaterial = parser["Separated Material"],
                            CoveringDates = Date.Parse(parser["Covering Dates"]).ToArray()
                        };
                    }
                }
            }
            
        }
    }
}