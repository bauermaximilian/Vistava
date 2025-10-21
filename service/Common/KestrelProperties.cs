// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Common;

public class KestrelProperties
{
    private const string PropertyPath = "Endpoints:MainEndpoint:Url";

    public string Endpoint
    {
        get => Configuration[PropertyPath] ?? "";
        set
        {
            Configuration[PropertyPath] = value;
            if (Configuration is IConfigurationRoot root)
            {
                root.Reload();
            }
        }
    }

    public IConfiguration Configuration { get; }

    public KestrelProperties()
    {
        Configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()
        {
            { "Endpoints:MainEndpoint:Url", "" }
        }).Build();
    }
}
