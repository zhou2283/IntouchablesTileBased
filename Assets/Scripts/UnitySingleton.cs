using UnityEngine;
using System.Collections;
using System.Threading;
/// <summary>
/// 单例基类，提供两个抽象函数Init 和 DisInit 初始化和逆初始化过程。
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnitySingleton<T> : MonoBehaviour
where T : UnitySingleton<T>
{
    protected static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = GameObject.Find("Singleton");
                if (go == null)
                {
                    go = new GameObject("Singleton");
                    DontDestroyOnLoad(go);
                }
                _instance = go.GetComponent<T>();
                if (_instance == null)
                {
                    _instance = go.AddComponent<T>();
                }
            }
            return _instance;
        }
        
    }
}