using Azure.Data.AppConfiguration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FeatureDashApi;

public class FeatureSettingsFunctions
{
    private readonly ILogger _logger;
    private readonly ConfigurationClient _configurationClient;

    public FeatureSettingsFunctions(ConfigurationClient configurationClient, ILogger<FeatureSettingsFunctions> logger)
    {
        _logger = logger;
        _configurationClient = configurationClient;
    }

    [Function(nameof(GetFeatureSettings))]
    public async Task<FeatureSettingModel[]> GetFeatureSettings([HttpTrigger(AuthorizationLevel.Function, "get", Route = "FeatureSettings")] HttpRequestData req)
    {
        var featureSettings = await GetAllFeatureSettings().ToListAsync();
        return featureSettings.Select(feature =>
                new FeatureSettingModel(feature.FeatureId, feature.Label, feature.Description, feature.IsEnabled, feature.LastModified))
            .ToArray();
    }

    [Function(nameof(GetLabels))]
    public async Task<string[]> GetLabels([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Labels")] HttpRequestData req)
    {
        var featureSettings = await GetAllFeatureSettings().ToListAsync();
        return featureSettings.Select(f => f.Label).Distinct().ToArray();
    }

    private async IAsyncEnumerable<FeatureFlagConfigurationSetting> GetAllFeatureSettings()
    {
        await foreach (var setting in _configurationClient.GetConfigurationSettingsAsync(new SettingSelector() { KeyFilter = ".appconfig.featureflag/*" }))
        {
            if (setting is FeatureFlagConfigurationSetting feature)
                yield return feature;
        }
    }

    [Function(nameof(SetFeatureSettings))]
    public async Task SetFeatureSettings([HttpTrigger(AuthorizationLevel.Function, "post", Route = "FeatureSettings/{featureId}")] HttpRequestData req, string featureId, string label)
    {
        var response = await _configurationClient.GetConfigurationSettingAsync($".appconfig.featureflag/{featureId}", label);
        if (response.Value is not FeatureFlagConfigurationSetting feature)
            return;

        feature.IsEnabled = !feature.IsEnabled;
        await _configurationClient.SetConfigurationSettingAsync(feature);
    }
}

public record FeatureSettingModel(string FeatureId, string Label, string Description, bool Enabled, DateTimeOffset? LastModified);
