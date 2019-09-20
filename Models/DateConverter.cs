using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KnowledgeGraphBuilder
{
    public class DateConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new DateContractResolver() };

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Date));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            switch (jo["Type"].Value<string>())
            {
                case "Year":
                    return JsonConvert.DeserializeObject<Date.Year>(jo.ToString(), SpecifiedSubclassConversion);
                case "Day":
                    return JsonConvert.DeserializeObject<Date.Day>(jo.ToString(), SpecifiedSubclassConversion);
                case "YearRange":
                    return JsonConvert.DeserializeObject<Date.YearRange>(jo.ToString(), SpecifiedSubclassConversion);
                case "DateRange":  
                    return JsonConvert.DeserializeObject<Date.DateRange>(jo.ToString(), SpecifiedSubclassConversion);
                case "Undated":
                    return new Date.Undated();
                case "Unparsed":
                    return JsonConvert.DeserializeObject<Date.Unparsed>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new Exception();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }
}