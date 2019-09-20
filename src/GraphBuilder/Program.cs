using System;
using System.Globalization;
using System.IO;
using System.Linq;
using KnowledgeGraphBuilder;
using Newtonsoft.Json;
using Pluralize.NET;

namespace Neo4JWriter
{
    class Program
    {
        public static ProcessedEntity ReadEntity(string filePath)
        {
            return JsonConvert.DeserializeObject<ProcessedEntity>(File.ReadAllText(filePath));
        }
        
        static void Main(string[] args)
        {
            var pluralizer = new Pluralizer();
            var db = new Neo4JDb();
            var fileLocation = @"Data\LIVE_C14242_3Sep2019_TSV.json";
            
            var parent = ReadEntity(fileLocation);
            
            db.Create("item", new {
                name = parent.Text.First(), 
                reference = parent.Reference.FirstOrDefault(), 
                parentReference = parent.ParentReference.FirstOrDefault(),
                startDate = new DateTime?(),
                endDate = new DateTime?()
            });
            
            foreach(var entity in parent.Entities.Where(e => e.Type.ToLower() != "number"))
            {
                var name = GetEntityName(entity, pluralizer);

                db.Create(entity.Type.ToLower(), new { name = name });
                db.Relate("item", "name", parent.Text.First(), entity.Type.ToLower(), entity.Type.ToLower(), "name", name);
            }

            foreach (var child in parent.Children)
            {
                var dates = child.Dates.Select(d => d.ToDateTime()).FirstOrDefault(d => d.HasValue);

                if (dates != null)
                {
                    var (start, end) = dates.Value;
                    db.Create("item", new
                    {
                        name = child.Text.First(),
                        reference = child.Reference.FirstOrDefault(),
                        parentReference = child.ParentReference.FirstOrDefault(),
                        startDate = start,
                        endDate = end
                    });
                    
                }
                else
                {
                    db.Create("item", new {
                        name = child.Text.First(),
                        reference = child.Reference.FirstOrDefault(),
                        parentReference = child.ParentReference.FirstOrDefault(),
                        startDate = new DateTime?(),
                        endDate = new DateTime?()
                    });
                }

                db.Relate("item", "reference", child.Reference.First(), "child_of", "item", "reference", parent.Reference.First());
                foreach(var entity in child.Entities)
                {
                    var name = GetEntityName(entity, pluralizer);
                    db.Create(entity.Type.ToLower(), new { name = name });
                    db.Relate("item", "reference", child.Reference.First(), entity.Type.ToLower(), entity.Type.ToLower(), "name", name);
                }

            }
            
            Console.WriteLine("Done");

        }

        private static string GetEntityName(Entity entity, Pluralizer pluralizer)
        {
            var txi = CultureInfo.CurrentCulture.TextInfo;
            var name = txi.ToTitleCase(entity.Name);

            switch (entity.Type.ToLower())
            {
                case "documenttype":
                case "location":
                {
                    name = pluralizer.Singularize(name);
                    break;
                }
            }

            return name;
        }
    }
}