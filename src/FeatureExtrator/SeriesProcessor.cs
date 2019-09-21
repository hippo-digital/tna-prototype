using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using edu.stanford.nlp.pipeline;
using GenericParsing;
using Newtonsoft.Json;

namespace KnowledgeGraphBuilder
{
    public static class SeriesProcessor
    {
        private static IEnumerable<Series> ReadSeries(string file, string tsvFolder)
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
                        Entries = ReadSeriesEntries(Path.Combine(tsvFolder, fileName)).ToArray()
                    };
                }
            }
        }

        private static IEnumerable<SeriesEntry> ReadSeriesEntries(string file)
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
        
        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented,
        };
        
        public static void ProcessSeriesEntries(StanfordCoreNLP pipeline, string TSVList, string TSVDir, DirectoryInfo output)
        {
            Console.WriteLine($"Started reading TSV at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
            var data = ReadSeries(TSVList, TSVDir);
            Console.WriteLine($"Finished reading TSV at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");

            foreach (var series in data)
            {
                var started = DateTime.Now;
                Console.WriteLine($"Started processing {series.File} started at {started.ToString(CultureInfo.InvariantCulture)}");
                var entity = pipeline.Classify(series.Title);
                entity.Reference = new[] {series.SeriesRef};

                var seriesNumber = 1;
                var seriesCount = series.Entries.Length;

                foreach (var entry in series.Entries)
                {
                    Console.WriteLine($"Started processing {seriesNumber} of {seriesCount} started at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
                    
                    var text = entry.Title + " " + entry.Description;
                    text = text.RemoveAll(entry.Reference, series.SeriesRef, entry.Reference + ".", series.SeriesRef + ".");
                    
                    var processed = pipeline.Classify(text);

                    processed.Reference = new[] {entry.Reference};
                    processed.ParentReference = entity.Reference;
                    processed.RelatedMaterial = entry.RelatedMaterial;
                    processed.SeparatedMaterial = entry.SeperatedMaterial;
                    processed.Dates = entry.CoveringDates;

                    entity.Children.Add(processed);
                    seriesNumber++;
                }

                var path = SaveToJsonFile(output, series, entity);

                var finished = DateTime.Now;
                Console.WriteLine(
                    $"Finished Processing {series.File} results: {path} finished at {finished.ToString(CultureInfo.InvariantCulture)} took {(finished.Subtract(started).TotalSeconds.ToString("F"))} secs");
            }
        }

        private static string SaveToJsonFile(DirectoryInfo output, Series series, ProcessedEntity entity)
        {
            var path = Path.GetFullPath(Path.Combine(output.FullName, Path.ChangeExtension(series.File, "json")));
            using (var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var sw = new StreamWriter(fs))
            using (var jw = new JsonTextWriter(sw))
            {
                Serializer.Serialize(jw, entity);
            }

            return path;
        }
    }
}