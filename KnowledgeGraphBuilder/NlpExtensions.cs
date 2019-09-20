using System.Linq;
using edu.stanford.nlp.pipeline;

namespace KnowledgeGraphBuilder
{
    public static class NlpExtensions
    {
        public static ProcessedEntity Classify(this StanfordCoreNLP nlp, string source)
        {
            CoreDocument document = new CoreDocument(source);
            nlp.annotate(document);

            return document.sentences()
                .toArray()
                .OfType<CoreSentence>()
                .Select(s => new ParsedSentence(s))
                .Aggregate(new ProcessedEntity(), (r, s) => ProcessedEntity.Union(r, s.ToProcessedEntity()));
        }
    }
}