using Azure.Data.AppConfiguration;

namespace FeatureDashApi;

public record FeatureFlagModel(string FeatureId, string Label, string Description, bool Enabled, DateTimeOffset? LastModified);


public static class ModelMapper
{
    public static FeatureFlagModel ToModel(this FeatureFlagConfigurationSetting feature)
    {
        return new FeatureFlagModel(feature.FeatureId, feature.Label, feature.Description, feature.IsEnabled, feature.LastModified);
    }

}