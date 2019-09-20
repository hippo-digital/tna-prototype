using System.Linq;
using edu.stanford.nlp.ie.util;
using edu.stanford.nlp.pipeline;
using java.lang;
using java.util;
using Neo4j.Driver.V1;

namespace KnowledgeGraphBuilder
{
    public class ParsedSentence
    {
        public Entity[] Entities =>
            Sentence.entityMentions().toArray()
                .OfType<CoreEntityMention>()
                .Select(e => new Entity
                {
                    Name = e.text(),
                    Type = e.entityType()
                }).ToArray();

        public object Mentions =>
            Sentence.coreMap().get(Class.forName("edu.stanford.nlp.ling.CoreAnnotations$MentionsAnnotation"))
                .As<ArrayList>()
                .toArray()
                .OfType<string>();
        
        public Triple[] Relations => 
            Sentence.relations().toArray()
                .OfType<RelationTriple.WithLink>()
                .Select(relation => new Triple
                {
                    Subject = relation.subjectLemmaGloss(),
                    Predicate = relation.relationGloss().Split(':')[1],
                    Object = relation.objectLemmaGloss()
                }).ToArray();
        
        public CoreSentence Sentence { get; set; }

        public ParsedSentence(CoreSentence s)
        {
            Sentence = s;
        }

        public ProcessedEntity ToProcessedEntity()
        {
            return new ProcessedEntity
            {
                Text = new []{Sentence.text()},
                Entities =  Entities,
                Triples = Relations
            };
        }
    }
}