using System;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver.V1;
using Newtonsoft.Json.Linq;

namespace KnowledgeGraphBuilder
{
    public class Neo4JDb : IDisposable
    {
        private readonly IDriver _driver;

        public Neo4JDb()
        {
            _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "admin"));
        }

        
        private IEnumerable<(string, string)> GetNodeProperties(object instance)
        {
            foreach (var prop in JObject.FromObject(instance))
            {
                if (prop.Value.Type != JTokenType.Null)
                {
                    switch (prop.Value.Type)
                    {
                        case JTokenType.Integer:
                            yield return (prop.Key, prop.Value.Value<int>().ToString());
                            break;
                        case JTokenType.Float:
                            yield return (prop.Key, prop.Value.Value<double>().ToString());
                            break;
                        case JTokenType.Date:
                            yield return (prop.Key, $"'{prop.Value.Value<DateTime>():s}'");
                            break;
                        default:
                            yield return (prop.Key, $"\"{prop.Value.Value<string>().Replace("'","\'")}\"");
                            break;
                    }
                }
            }   
        }
        
        public void Create(string type, object instance)
        {
            using (var session = _driver.Session())
            {
                var node = session.WriteTransaction(tx =>
                {

                    var properties =
                        String.Join(",", GetNodeProperties(instance).Select(p => $"{p.Item1}:{p.Item2}"));

                    var result = tx.Run($"MERGE (a:{type} {{ {properties} }}) RETURN id(a)");
                    return result.Single()[0].As<long>();
                });
            }
        }
        
        public void Relate(
            string firstType, string firstProp, string firstValue, 
            string relation, 
            string secType, string secProp, string secValue)
        {
            using (var session = _driver.Session())
            {
                var node = session.WriteTransaction(tx =>
                {
                    var result = tx.Run($"MATCH (a:{firstType}),(b:{secType}) " +
                                        $"WHERE a.{firstProp} = \"{firstValue.Replace("'","\'")}\" AND b.{secProp} = \"{secValue.Replace("'","\'")}\" " +
                                        $"CREATE (a)-[r:{relation}]->(b)\nRETURN type(r)");
                    return result;
                });
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }
}