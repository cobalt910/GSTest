namespace NetworkFramework
{
	public abstract class Singleton<T> : UnityEngine.MonoBehaviour where T : UnityEngine.Object
	{
		private static bool _isCalled;
		
		private static T _instance;
		public static T Instance()
		{
			if (_instance == null && _isCalled)
			{
				_instance = FindObjectOfType<T>();
				_isCalled = false; // todo: maybe error
			}
			return _instance;
		}

		protected virtual void Awake()
		{
			_instance = FindObjectOfType<T>();
		}
	}
}