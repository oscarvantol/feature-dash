using Azure.Data.AppConfiguration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

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
        var result = new List<FeatureSettingModel>();

        await foreach (ConfigurationSetting setting in _configurationClient.GetConfigurationSettingsAsync(new SettingSelector() { KeyFilter = ".appconfig.featureflag/*" }))
        {
            _logger.LogInformation(setting.Key);
            if (setting is FeatureFlagConfigurationSetting feature)
                result.Add(new FeatureSettingModel(feature.FeatureId, feature.Label, feature.Description, feature.IsEnabled, feature.LastModified));
        }
        return result.ToArray();
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
