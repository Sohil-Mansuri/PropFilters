using System.Collections;
using System.Reflection;

namespace PropFilters.PropFilter
{
    public class NestedFieldProjector
    {
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropCache = [];

        public object Project(object source, FilterNode tree)
        {
            if (source is null || tree is null) return source;
            return ProjectObject(source, tree);
        }

        public List<object> ProjectList<T>(List<T> source, FilterNode tree)
        {
            if (source is null) return [];
            if (tree is null) return [.. source.Cast<object>()];
            return [.. source.Select(item => Project(item, tree))];
        }

        private object ProjectObject(object source, FilterNode tree)
        {
            if (source is null) return null;

            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var props = GetCachedProps(source.GetType());

            foreach (var (key, childTree) in tree.Children)
            {
                if (!props.TryGetValue(key, out var prop)) continue;

                object value;
                try
                {
                    value = prop.GetValue(source);
                }
                catch
                {
                    continue;
                }

                result[key] = childTree is null
                    ? value
                    : ProjectValue(value, childTree);
            }

            return result;
        }

        private object ProjectValue(object value, FilterNode tree)
        {
            if (value is null) return null;

            if (value is IDictionary) return value;

            if (value is IEnumerable enumerable && value is not string)
            {
                var items = enumerable.Cast<object>().ToList();

                if (tree.SliceCount.HasValue)
                {
                    var count = Math.Min(tree.SliceCount.Value, items.Count);
                    items = tree.FromEnd
                        ? [.. items.Skip(items.Count - count)]
                        : [.. items.Take(count)];
                }

                if (tree.IsLeaf)
                    return items;

                return items
                    .Select(item => item is null ? null : ProjectObject(item, tree))
                    .ToList();
            }

            if (IsPrimitive(value.GetType())) return value;

            return ProjectObject(value, tree);
        }

        private static bool IsPrimitive(Type t) =>
            t.IsPrimitive || t == typeof(string) || t == typeof(decimal) ||
            t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid) ||
            (Nullable.GetUnderlyingType(t) is { } u && IsPrimitive(u));

        private static Dictionary<string, PropertyInfo> GetCachedProps(Type type)
        {
            if (PropCache.TryGetValue(type, out var cached)) return cached;
            var dict = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            PropCache[type] = dict;
            return dict;
        }
    }
}
