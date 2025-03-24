namespace Led3D_2.Utility;

public class Singleton<T> where T : new()
{
    private static T? _instance;
    private static readonly Lock LockObject = new();

    // Prevent external instantiation
    protected Singleton() { }

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;
            lock (LockObject)
            {
                _instance ??= new T();
                return _instance;
            }
        }
    }
}