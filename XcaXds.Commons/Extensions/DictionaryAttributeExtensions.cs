namespace XcaXds.Commons.Extensions;

public static class DictionaryAttributeExtensions
{
    extension<T>(Dictionary<string, List<T>> dictionary)
    {
        public void AddOrUpdate(string key, List<T> value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].AddRange(value);
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public void AddRange(Dictionary<string, List<T>> values)
        {
            foreach (var value in values)
            {
                dictionary.Add(value.Key, value.Value);
            }
        }


        public Dictionary<string, T?> IndexAttributesWithPrefix(string prefix)
        {
            return dictionary
                .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.FirstOrDefault(),
                    StringComparer.InvariantCultureIgnoreCase);
        }
    }
}