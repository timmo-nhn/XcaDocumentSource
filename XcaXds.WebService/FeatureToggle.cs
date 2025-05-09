

using Microsoft.FeatureManagement;

namespace XcaXds.Commons.Models;

public class FeatureToggle
{
    private readonly IVariantFeatureManager _featureManager;

    public FeatureToggle(IVariantFeatureManager featureManager)
    {
        _featureManager = featureManager;
    }
}