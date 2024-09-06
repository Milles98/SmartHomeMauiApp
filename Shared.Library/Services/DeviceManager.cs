using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace Shared.Library.Services;

public class DeviceManager
{
	private readonly RegistryManager _registryManager;
	private readonly ServiceClient _serviceClient;

	public DeviceManager(string connectionString)
	{
		_registryManager = RegistryManager.CreateFromConnectionString(connectionString);
		_serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
	}

	public async Task GetDevices(string query)
	{
		var q = _registryManager.CreateQuery(query);
		var deviceTwins = new List<Twin>();

		foreach (var twin in await q.GetNextAsTwinAsync())
		{
			deviceTwins.Add(twin);
		}
	}
}
