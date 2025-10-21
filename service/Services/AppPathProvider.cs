// SPDX-License-Identifier: GPL-3.0-or-later

using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Vistava.Service.Services;

public class AppPathProvider(IServer server, ApplicationParameters appParameters)
{
    private const int AddressTimeoutMs = 5000;
    
    public async Task<string> GetAppUrlLocal(CancellationToken stoppingToken)
    {
        int appPort = await GetApplicationPort(AddressTimeoutMs, stoppingToken);
        return BuildUri("127.0.0.1", appPort).ToString();
    }

    public async Task<IEnumerable<string>> GetAppUrlsExternal(CancellationToken stoppingToken)
    {
        int appPort = await GetApplicationPort(AddressTimeoutMs, stoppingToken);
        List<string> addresses = [];
        foreach (string address in GetInterfaceAddresses())
        {
            addresses.Add(BuildUri(address, appPort).ToString());
        }
        return addresses;
    }
    
    public async Task<int> GetApplicationPort(int timeoutMs, CancellationToken stoppingToken)
    {
        var addresses = server.Features.Get<IServerAddressesFeature>();
        var start = DateTime.UtcNow;
        while ((addresses?.Addresses.Count ?? -1) == 0 && 
               (timeoutMs == 0 || (DateTime.UtcNow - start).TotalMilliseconds < timeoutMs) &&
               !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(25, stoppingToken);
        }
        
        stoppingToken.ThrowIfCancellationRequested();
        
        if ((addresses?.Addresses.Count ?? 0) > 0 && 
            Uri.TryCreate(addresses?.Addresses.FirstOrDefault(), UriKind.Absolute, out var addressUri))
        {
            return addressUri.Port;
        }
        else
        {
            throw new TimeoutException("The server did not provide the required address information " +
                "within the expected time.");
        }
    }

    private Uri BuildUri(string host, int port) => new UriBuilder
    {
        Scheme = "http",
        Host = host,
        Path = appParameters.Path,
        Port = port
    }.Uri;
    
    private IEnumerable<string> GetInterfaceAddresses() =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(i => i.OperationalStatus == OperationalStatus.Up && 
                (i.NetworkInterfaceType is NetworkInterfaceType.Ethernet or 
                    NetworkInterfaceType.Ethernet3Megabit or NetworkInterfaceType.FastEthernetFx or 
                    NetworkInterfaceType.FastEthernetT or NetworkInterfaceType.GigabitEthernet or 
                    NetworkInterfaceType.Wireless80211))
            .Select(i =>
                i.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address)
            .Where(a => a != null)
            .Select(a => a?.ToString() ?? ""); 
}
