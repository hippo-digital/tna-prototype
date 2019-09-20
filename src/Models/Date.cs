using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KnowledgeGraphBuilder
{
    [JsonConverter(typeof(DateConverter))]
    public abstract class Date
    {
        public abstract string Type { get; }
        
        public sealed class Year : Date
        {
            public override string Type => nameof(Year);

            public int Value { get; }

            public Year(int year)
            {
                Value = year;
            }

            public static bool TryParse(string value, out Year result)
            {
                if (int.TryParse(value.Trim(), out var i))
                {
                    result = new Year(i);
                    return true;
                }

                result = null;
                return false;
            }

            
        }
        
        public sealed class Day : Date
        {
            public override string Type => nameof(Day);
            public DateTime Value { get; }

            public Day(DateTime dt)
            {
                Value = dt;
            }

            public static bool TryParse(string value, out Day result)
            {
                if (DateTime.TryParse(value.Trim(), out var i))
                {
                    result = new Day(i);
                    return true;
                }

                result = null;
                return false;
            }
        }

        public sealed class YearRange : Date
        {
            public override string Type => nameof(YearRange);
            public Year Start { get; }
            public Year End { get; }

            public YearRange(Year start, Year end = null)
            {
                Start = start;
                End = end;
            }

            public static bool TryParse(string value, out YearRange result)
            {
                if (value.Contains('-'))
                {
                    var years = value.Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
                    if (years.Length == 2 && Year.TryParse(years[0], out var s) && Year.TryParse(years[1], out var e))
                    {
                        result = new YearRange(s, e);
                        return true;
                    }

                    if (years.Length == 1 && Year.TryParse(years[0], out var s1))
                    {
                        result = new YearRange(s1);
                        return true;
                    }
                }

                result = null;
                return false;
            }
        }

        public sealed class DateRange : Date
        {
            public override string Type => nameof(DateRange);
            
            private static string[] formats =
            {
                "yyyy",
                "yyyy dd MMM",
                "yyyy MMMM",
                "yyyy MMM",
                "yyyy MMM dd",
                "yyyy MMM d",
                "yyyy MMMM dd",
                "yyyy MMMM d",
                "MMM dd yyyy",
                "MMMM dd yyyy",
                "dd MM yyyy",
                "dd MMM yyyy",
                "dd MMMM yyyy",
                "d MM yyyy",
                "d MMM yyyy",
                "d MMMM yyyy",
            };

            public DateTime Start { get; }
            public DateTime? End { get; }

            public DateRange(DateTime start, DateTime? end = null)
            {
                Start = start;
                End = end;
            }

            public static bool TryParse(string value, out DateRange result)
            {
                if (value.Contains('-'))
                {
                    var years = value.Split(new [] {'-' }, StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToArray();

                    if (years.Length == 3)
                    {
                        var newY = new string[2];
                        newY[0] = years[0] + " " + years[1];
                        newY[1] = years[2];
                        years = newY;
                    }
                    
                    if (years.Length == 2)
                    {
                        if (DateTime.TryParseExact(years[0], formats, CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var s))
                        {
                            if (DateTime.TryParseExact(years[1], formats, CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var e))
                            {
                                result = new DateRange(s, e);
                                return true;
                            }

                            if (DateTime.TryParseExact(years[1], new []{ "MMM", "MMMM" }, CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var eMonth))
                            {
                                var newE = new DateTime(s.Year, eMonth.Month, eMonth.Day);
                                result = new DateRange(s, newE);
                                return true;
                            }
                        }
                    }

                    if (years.Length == 1 && DateTime.TryParseExact(years[0].TrimEnd('.'), formats,
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var s1))
                    {
                        result = new DateRange(s1);
                        return true;
                    }
                }

                result = null;
                return false;
            }
        }

        public sealed class Undated : Date
        {
            public override string Type => nameof(Undated);
            
            public static bool TryParse(string value, out Undated result)
            {
                if (String.IsNullOrWhiteSpace(value) || value.Equals("undated", StringComparison.OrdinalIgnoreCase))
                {
                    result = new Undated();
                    return true;
                }

                result = null;
                return false;
            }
        }

        public sealed class Unparsed : Date
        {
            public override string Type => nameof(Unparsed);
            
            public string Value { get; }

            public Unparsed(string value)
            {
                Value = value;
            }
        }
        
        public static Date ParseOne(string value)
        {
            var v = value.Trim(' ', '[', ']')
                .Replace(".", " ")
                .Replace("  ", " ")
                .Replace("Sept", "Sep")
                .Replace("Apl", "Apr")
                .Replace("Aprl", "Apr");

            if (Undated.TryParse(v, out var u)) return u;

            if (DateRange.TryParse(v, out var dr)) return dr;

            if (YearRange.TryParse(v, out var yrr)) return yrr;
            
            if (Year.TryParse(v, out var yr)) return yr;

            if (Day.TryParse(v, out var dt)) return dt;
            

            return new Unparsed(value);
        }
        
        public static IEnumerable<Date> Parse(string value)
        {
            if (value.Contains(';'))
            {
                foreach (var dt in value.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return ParseOne(dt);
                }
            }
            else
            {
                yield return ParseOne(value);
            }
        }

        public (DateTime, DateTime?)? ToDateTime()
        {
            switch (this)
            {
                case Year y when y.Value > 0: return (new DateTime(y.Value, 1, 1), null);
                case Day d: return (d.Value, null);
                case YearRange y: return (new DateTime(y.Start.Value, 1, 1), new DateTime(y.End.Value, 1, 1));
                case DateRange dr: return (dr.Start, dr.End);
                default:
                    return null;
            }
        }
    }
    
    
    public class DateContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Date).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }
}