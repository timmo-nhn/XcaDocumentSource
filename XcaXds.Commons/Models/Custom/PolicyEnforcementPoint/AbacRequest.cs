public class AbacRequest
{
    public AbacRequest()
    {
        Attributes ??= [];
    }

    public AbacRequest(params KeyValuePair<string,string>[] attributes)
    {
        Attributes ??= [];
        
        foreach (var attribute in attributes)
        {
            Attributes.Add(attribute.Key, [attribute.Value]);
        }
    }

    public AbacRequest(KeyValuePair<string,string> attribute)
    {
        Attributes ??= [];
        Attributes.Add(attribute.Key, [attribute.Value]);
    }

    public Dictionary<string, List<string>> Attributes { get; set; }
}