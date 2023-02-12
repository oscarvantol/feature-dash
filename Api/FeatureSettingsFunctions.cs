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
        var result = new List<FeatureSettingModel>();

        await foreach (FeatureFlagConfigurationSetting feature in _configurationClient.GetConfigurationSettingsAsync(new SettingSelector() { KeyFilter = ".appconfig.featureflag/*" }))
        {
            result.Add(new FeatureSettingModel(feature.FeatureId, feature.Label, feature.Description, feature.IsEnabled, feature.LastModified));
        }
        return result.ToArray();
    }
}

public record FeatureSettingModel(string FeatureId, string Label, string Description, bool Enabled, DateTimeOffset? LastModified);
