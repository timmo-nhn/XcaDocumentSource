namespace XcaXds.Commons.Models.Custom.PolicyDtos;

public class PolicySet
{
    public PolicySet()
    {
        SetId = Guid.NewGuid().ToString();
    }

    public string SetId { get; set; }
    public List<AbacPolicy>? Policies { get; set; }
}