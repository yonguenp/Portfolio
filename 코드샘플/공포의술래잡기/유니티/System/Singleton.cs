public class Singleton<T> where T : class, new()
{
    protected static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();

            }
            return _instance;
        }
    }

    public virtual bool Init()
    {
        return true;
    }

    public virtual bool Remove()
    {
        return true;
    }
}