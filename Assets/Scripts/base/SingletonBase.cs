using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : Component {
    private static T _instance;
    private static object _lock = new object();
    private static bool applicationIsQuitting = false;
    
    public static T Instance {
        get {
            if (applicationIsQuitting) {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }
            else {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = FindObjectOfType(typeof(T)) as T;

                        if (_instance == null) {
                            //  Create a new GameObject and attach this component to it
                            GameObject singleton = new GameObject("Singleton: " + typeof(T).ToString());
                            _instance = singleton.AddComponent<T>();

                            //  Make Singleton be persistent across the unity scenes
                            DontDestroyOnLoad(singleton);
                            Debug.Log(string.Format("Create singleton: {0}", typeof(T).ToString()));
                        } else {
                            Debug.Log(string.Format("Singleton: {0} has already been created", typeof(T).ToString()));
                        }
                    }
                    return _instance;
                }
            }
        }
    }

    public void OnDestroy() {
        applicationIsQuitting = true;
    }
}
