using System;
using System.Globalization;
using System.IO;
using System.Linq;
using edu.stanford.nlp.pipeline;
using java.util;
using Newtonsoft.Json;

namespace KnowledgeGraphBuilder
{
    class Program
    {
        private const string TSVDir = @"Data\NursingSeries_TSVs";
        private const string TSVList = @"Data\NursingSeries.txt";
        private const string JarRoot = @"stanford-corenlp-full-2016-10-31\stanford-corenlp-3.7.0-models\";
        
        static void Main(string[] args)
        {
            Console.WriteLine($"Started reading TSV at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}"); 
            var data = SeriesReader.ReadHeaders(TSVList, TSVDir);
            Console.WriteLine($"Finished reading TSV at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}"); 
             
            Console.WriteLine($"Started loading NLP at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}"); 
            
            Properties props = new Properties();
            props.setProperty("annotators", "tokenize,ssplit,pos,depparse,lemma,ner,coref,kbp");
            props.setProperty("ner.useSUTime", "0");

            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(JarRoot);
            var pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);
            
            Console.WriteLine($"Finished loading NLP at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}"); 
            
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
            };

            foreach(var series in data.Where(s => s.File.Contains("C14242_3Sep2019")))
            {
                var started = DateTime.Now;
                Console.WriteLine(
                    $"Started processing {series.File} started at {started.ToString(CultureInfo.InvariantCulture)}");
                var entity = pipeline.Classify(series.Title);
                entity.Reference = new[] {series.SeriesRef};

                var seriesNumber = 1;
                var seriesCount = series.Entries.Length;

                foreach (var entry in series.Entries)
                {
                    Console.WriteLine(
                        $"Started processing {seriesNumber} of {seriesCount} started at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
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

                var path = Path.Combine(Environment.CurrentDirectory, Path.ChangeExtension(series.File, "json"));
                using (var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs))
                using (var jw = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jw, entity);
                }

                var finished = DateTime.Now;
                Console.WriteLine($"Wrote: ");
                Console.WriteLine(
                    $"Finished Processing {series.File} results: {path} finished at {finished.ToString(CultureInfo.InvariantCulture)} took {(finished.Subtract(started).TotalSeconds.ToString("F"))} secs");
            }
            
            
            Console.WriteLine("Finished.");
        }
    }

}
