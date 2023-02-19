using Azure.Data.AppConfiguration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FeatureDashApi;

public class FeatureFlagsFunctions
{
    private readonly ILogger _logger;
    private readonly ConfigurationClient _configurationClient;

    public FeatureFlagsFunctions(ConfigurationClient configurationClient, ILogger<FeatureFlagsFunctions> logger)
    {
        _logger = logger;
        _configurationClient = configurationClient;
    }

    [Function(nameof(GetFeatureFlags))]
    public async Task<FeatureFlagModel[]> GetFeatureFlags([HttpTrigger(AuthorizationLevel.Function, "get", Route = "FeatureFlags")] HttpRequestData req)
    {
        var featureSettings = await GetAllFeatureSettings().ToListAsync();
        return featureSettings.Select(feature => feature.ToModel()).ToArray();
    }

    private async IAsyncEnumerable<FeatureFlagConfigurationSetting> GetAllFeatureSettings()
    {
        await foreach (var setting in _configurationClient.GetConfigurationSettingsAsync(new SettingSelector() { KeyFilter = ".appconfig.featureflag/*" }))
        {
            if (setting is FeatureFlagConfigurationSetting feature)
                yield return feature;
        }
    }

    [Function(nameof(SetFeatureFlagIsEnabled))]
    public async Task SetFeatureFlagIsEnabled([HttpTrigger(AuthorizationLevel.Function, "post", Route = "FeatureFlags/{featureId}/IsEnabled")] HttpRequestData req, string featureId, string label)
    {
        var value = await req.ReadFromJsonAsync<bool>();

        var response = await _configurationClient.GetConfigurationSettingAsync($".appconfig.featureflag/{featureId}", label);
        if (response.Value is not FeatureFlagConfigurationSetting feature)
            return;

        if (feature.IsEnabled == value)
            return;

        feature.IsEnabled = value;


        await _configurationClient.SetConfigurationSettingAsync(feature);
    }
}

