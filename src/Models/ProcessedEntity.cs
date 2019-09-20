using System.Collections.Generic;
using System.Linq;

namespace KnowledgeGraphBuilder
{
    public class ProcessedEntity
    {
        public string[] Text { get; set; } = { };
        public string[] Reference { get; set; } = { };
        public string[] ParentReference { get; set; } = { };
        public Entity[] Entities { get; set; } = { };
        public Triple[] Triples { get; set; } = { };
        public IList<ProcessedEntity> Children { get; set; } = new List<ProcessedEntity>();
        public string RelatedMaterial { get; set; }
        public string SeparatedMaterial { get; set; }
        public Date[] Dates { get; set; } = { };

        public static ProcessedEntity Union(ProcessedEntity a, ProcessedEntity b)
        {
            return new ProcessedEntity
            {
                Text = a.Text.Union(b.Text).ToArray(),
                Reference = a.Reference.Union(b.Reference).ToArray(),
                ParentReference = a.ParentReference.Union(b.ParentReference).ToArray(),
                Entities = a.Entities.Union(b.Entities).ToArray(),
                Triples = a.Triples.Union(b.Triples).ToArray(),
                Children = a.Children.Union(b.Children).ToList(),
                Dates = a.Dates.Union(b.Dates).ToArray()
            };
        }
    }
}