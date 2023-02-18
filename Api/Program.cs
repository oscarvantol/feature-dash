using Azure.Core;
using Azure.Data.AppConfiguration;

using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((b, s) =>
    {
        s.AddSingleton<TokenCredential>(sb => b.HostingEnvironment.IsDevelopment() ? new AzureCliCredential() : new ManagedIdentityCredential());
        s.AddSingleton(sb => new ConfigurationClient(new Uri(b.Configuration["AppConfigurationUrl"]!), sb.GetService<TokenCredential>()));
    })
.Build();

host.Run();
