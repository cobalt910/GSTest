using System;
using System.Collections.Generic;
using System.Linq;
using NetworkFramework.Attributes.EasyButtons;
using NetworkFramework.Core;
using NetworkFramework.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NetworkFramework.Managers
{
	public class InstanceIdManager : Singleton<InstanceIdManager>
	{
		[SerializeField]
		private List<NetworkBehaviour> _networkBehaviours = new List<NetworkBehaviour>();
		public List<NetworkBehaviour> NetworkBehaviours => _networkBehaviours;
		
		private List<int> _ignoreInts = new List<int>();

		private int GetUniqueInstanceId()
		{
			while (true)
			{
				var value = Random.Range(0, 1000000000);

				if (NetworkManager.Instance() != null &&
				    NetworkManager.Instance().IgnoreInts.Contains(value))
					continue;
				
				if(_networkBehaviours.Exists(x => x.InstanceId == value))
					continue;

				return value;
			}
		}
		
		[Button]
		public void FindAllInstancesInScene()
		{
			_ignoreInts.Clear();
			foreach (var command in (NetworkCommandTypes[]) Enum.GetValues(typeof(NetworkCommandTypes)))
				_ignoreInts.Add((int) command);
			
			_networkBehaviours.Clear();
			var objs = FindObjectsOfType<NetworkBehaviour>();

			foreach (var obj in objs)
			{
				obj.InstanceId = GetUniqueInstanceId();
				obj.OwnerType = NetworkBehaviour.NetworkOwnerType.StaticInScene;
				_networkBehaviours.Add(obj);
			}
		}

		[Button]
		public void CheckBehaviours()
		{
			var objs = FindObjectsOfType<NetworkBehaviour>().ToList();
			if(objs.Count == 0) Debug.Log("Behaviours count = 0");
			else
			{
				int instanceIdMissing = 0;
				int nulls = 0;
				objs.ForEach(x =>
				{
					if (x == null) nulls++;
					if (x != null && x.InstanceId == 0)
					{
						instanceIdMissing++;
						Debug.Log(x.gameObject.name);
					}
				});
				if(nulls != 0) Debug.Log("InstanceIdManager: " + nulls + " objects is null!");
				if(instanceIdMissing != 0) Debug.Log("InstanceIdManager: NetworkIdMissing in " + instanceIdMissing + " objects!");
				if(nulls == 0 && instanceIdMissing == 0) Debug.Log("InstanceIdManager: All Fine.");
			}
		}

		[SerializeField] private int _debugId;
		[Button]
		public void DebugId()
		{
			var index = _networkBehaviours.FindIndex(x => x.InstanceId == _debugId);
			Debug.Log(index);
		}
	}
}
