using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <summary>
	/// Represents a configuration set of UIs that can be managed together in the <seealso cref="UiService"/>
	/// This can be helpful for a UI combo set that are always visible together (ex: player Hud with currency & settings)
	/// </summary>
	[Serializable]
	public struct UiSetConfig
	{
		public int SetId;
		public IReadOnlyList<Type> UiConfigsType;
	}

	/// <summary>
	/// Necessary to serialize the data in scriptable object
	/// </summary>
	[Serializable]
	public struct UiSetConfigSerializable
	{
		public int SetId;
		public List<string> UiConfigsAddress;

		public static UiSetConfig ToUiSetConfig(UiSetConfigSerializable serializable, List<UiConfigs.UiConfigSerializable> configs)
		{
			var types = new List<Type>();
			foreach (var address in serializable.UiConfigsAddress)
			{
				var config = configs.Find(c => c.AddressableAddress == address);
				if (!string.IsNullOrEmpty(config.UiType))
				{
					types.Add(Type.GetType(config.UiType));
				}
			}
			return new UiSetConfig 
			{ 
				SetId = serializable.SetId, 
				UiConfigsType = types.AsReadOnly() 
			};
		}

		public static UiSetConfigSerializable FromUiSetConfig(UiSetConfig config, List<UiConfigs.UiConfigSerializable> configs)
		{
			var addresses = new List<string>();
			foreach (var type in config.UiConfigsType)
			{
				var uiConfig = configs.Find(c => c.UiType == type.AssemblyQualifiedName);
				if (!string.IsNullOrEmpty(uiConfig.AddressableAddress))
				{
					addresses.Add(uiConfig.AddressableAddress);
				}
			}
			return new UiSetConfigSerializable 
			{ 
				SetId = config.SetId, 
				UiConfigsAddress = addresses 
			};
		}
	}
}