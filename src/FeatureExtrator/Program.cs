using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using edu.stanford.nlp.pipeline;
using java.util;
using Newtonsoft.Json;

namespace KnowledgeGraphBuilder
{
    class Program
    {
        private const string TSVDir = @"Data\NursingSeries_TSVs";
        private const string TSVList = @"Data\NursingSeries.txt";
        private const string JarRoot = @"..\..\..\stanford-corenlp-full-2016-10-31\stanford-corenlp-3.7.0-models\";
        private const string OutputPath = @"..\..\..\..\..\processed";
        
        static void Main(string[] args)
        {
            
             
            Console.WriteLine($"Started loading NLP at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}"); 
            
            Properties props = new Properties();
            props.setProperty("annotators", "tokenize,ssplit,pos,depparse,lemma,ner,coref,natlog,openie,kbp");
            props.setProperty("ner.useSUTime", "0");

            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(JarRoot);
            var pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);
            
            Console.WriteLine($"Finished loading NLP at {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
            var output = Directory.CreateDirectory(Path.GetFullPath(OutputPath));
            
            SeriesProcessor.ProcessSeriesEntries(pipeline,TSVList, TSVDir, output);


            Console.WriteLine("Finished.");
        }

        
    }

}
