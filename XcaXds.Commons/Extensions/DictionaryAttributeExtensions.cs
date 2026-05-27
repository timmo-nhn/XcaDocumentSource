public static class DictionaryAttributeExtensions
{
    extension(Dictionary<string, List<string>> dictionary)
    {
        public void AddOrUpdate(string key, List<string> value)
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

        public void AddRange(Dictionary<string, List<string>> values)
        {
            foreach (var value in values)
            {
                dictionary.Add(value.Key, value.Value);
            }
        }


        public Dictionary<string, string> IndexAttributesWithPrefix(string prefix)
        {
            return dictionary
                .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.FirstOrDefault() ?? string.Empty,
                    StringComparer.InvariantCultureIgnoreCase);
        }
        // public void GetValueOrDefault
    }
}