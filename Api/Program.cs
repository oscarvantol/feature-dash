using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((b, s) =>
        {
            s.AddSingleton<ConfigurationClient>(x => new ConfigurationClient(b.Configuration.GetConnectionString("AppConfiguration")));
        })
    .Build();

host.Run();
