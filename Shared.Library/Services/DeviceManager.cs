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

	public async Task<IEnumerable<Twin>> GetDevices(string query)
	{
		try
		{
			var q = _registryManager.CreateQuery(query);
			return await q.GetNextAsTwinAsync();
		}
		catch
		{
			//Bättre med responseresult klassen här !
			return null!;
		}
	}
}
