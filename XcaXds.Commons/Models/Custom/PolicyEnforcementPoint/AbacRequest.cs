public class AbacRequest
{
    public AbacRequest()
    {
        Attributes ??= [];
    }

    public Dictionary<string, List<string>> Attributes { get; set; }
}