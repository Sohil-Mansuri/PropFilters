namespace PropFilters.PropFilter
{
    public class FilterNode
    {
        public Dictionary<string, FilterNode> Children { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool IsLeaf => Children.Count == 0;

        public int? SliceCount { get; set; }
        public bool FromEnd { get; set; }
    }
}
