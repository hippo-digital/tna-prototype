namespace KnowledgeGraphBuilder
{
    public class Series
    {
        public string File { get; set; }
        public string SeriesRef { get; set; }
        public string Title { get; set; }
        
        public SeriesEntry[] Entries { get; set; }
    }
    
    public class SeriesEntry
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Date[] CoveringDates { get; set;}
        public string SeperatedMaterial { get; set; }
        public string RelatedMaterial { get; set; }
    }
}