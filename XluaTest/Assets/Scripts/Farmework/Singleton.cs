using UnityEngine;
using System.Collections;


public abstract class Singleton<T> where T : class, new()
{
    private static T _instance;

    public static T I
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
}
public class UnityNormalSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T I
    {
        get
        {
            if (_instance == null)
            {
                if ((_instance = Object.FindObjectOfType<T>()) == null)
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    _instance = go.AddComponent<T>();
                }
            }

            return _instance;
        }
    }
}




public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T I
    {
        get
        {
            if (_instance == null)
            {
                if ((_instance = Object.FindObjectOfType<T>()) == null)
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    _instance = go.AddComponent<T>();
                }

                UnityEngine.Object.DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }
}
