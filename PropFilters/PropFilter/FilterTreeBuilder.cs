namespace PropFilters.PropFilter
{
    public static class FilterTreeBuilder
    {
        private const string HotelPrefix = "hotel";
        private const string ContentPrefix = "content";

        public static (FilterNode HotelTree, FilterNode ContentTree) BuildFromPropFilters(
            IEnumerable<string> propFilters)
        {
            if (propFilters is null) return (null, null);

            var hotelFields = new List<string>();
            var contentFields = new List<string>();

            foreach (var filter in propFilters.Where(f => !string.IsNullOrWhiteSpace(f)))
            {
                var normalized = filter.Trim();

                if (normalized.StartsWith(HotelPrefix + ".", StringComparison.OrdinalIgnoreCase))
                    hotelFields.Add(normalized[(HotelPrefix.Length + 1)..]);

                else if (normalized.StartsWith(ContentPrefix + ".", StringComparison.OrdinalIgnoreCase))
                    contentFields.Add(normalized[(ContentPrefix.Length + 1)..]);
            }

            return (
                hotelFields.Count > 0 ? Build(hotelFields) : null,
                contentFields.Count > 0 ? Build(contentFields) : null
            );
        }

        private static FilterNode Build(IEnumerable<string> fields)
        {
            var root = new FilterNode();
            foreach (var field in fields)
            {
                try
                {
                    var parts = ParseParts(field);
                    InsertPath(root, parts, 0);
                }
                catch
                {
                    // Malformed field → skip silently
                }
            }
            return root;
        }

        private static List<PathPart> ParseParts(string field)
        {
            var result = new List<PathPart>();
            var segments = field.Split('.', StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                var bracketStart = segment.IndexOf('[');

                if (bracketStart < 0)
                {
                    result.Add(new PathPart(segment));
                    continue;
                }

                var name = segment[..bracketStart];
                var annotation = segment[(bracketStart + 1)..].TrimEnd(']').Trim();

                int? sliceCount = null;
                bool fromEnd = false;

                if (!string.IsNullOrEmpty(annotation))
                {
                    if (annotation.StartsWith('^'))
                    {
                        if (int.TryParse(annotation[1..], out var n) && n > 0)
                        { sliceCount = n; fromEnd = true; }
                    }
                    else
                    {
                        if (int.TryParse(annotation, out var n) && n > 0)
                        { sliceCount = n; fromEnd = false; }
                    }
                }

                result.Add(new PathPart(name, sliceCount, fromEnd));
            }

            return result;
        }

        private static void InsertPath(FilterNode node, List<PathPart> parts, int index)
        {
            if (index >= parts.Count) return;

            var part = parts[index];
            var key = part.Name;
            var isLast = index == parts.Count - 1;

            if (isLast)
            {
                if (part.SliceCount.HasValue)
                {
                    if (!node.Children.TryGetValue(key, out var existing) || existing is null)
                    {
                        existing = new FilterNode();
                        node.Children[key] = existing;
                    }
                    existing.SliceCount ??= part.SliceCount;
                    existing.FromEnd = part.FromEnd;
                }
                else
                {
                    if (!node.Children.ContainsKey(key))
                        node.Children[key] = null;
                }
            }
            else
            {
                if (!node.Children.TryGetValue(key, out var child) || child is null)
                {
                    child = new FilterNode();
                    node.Children[key] = child;
                }

                if (part.SliceCount.HasValue)
                {
                    child.SliceCount ??= part.SliceCount;
                    child.FromEnd = part.FromEnd;
                }

                InsertPath(child, parts, index + 1);
            }
        }

        private record PathPart(string Name, int? SliceCount = null, bool FromEnd = false);
    }
}
