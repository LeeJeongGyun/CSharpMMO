using ServerCore;
using UnityEngine;
using UnityEngine.UIElements;

public class Managers : MonoBehaviour
{
    private static Managers s_instance; // 유일성이 보장된다

    public static Managers Instance
    { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    #region Contents

    private MapManager _map = new MapManager();
    private ObjectManager _object = new ObjectManager();
    private NetworkManager _network = new NetworkManager();
    private InventoryManager _inventory = new InventoryManager();
    private WebManager _web = new WebManager();
    public static MapManager Map => Instance._map;
    public static ObjectManager Object => Instance._object;

    public static NetworkManager Network => Instance._network;

    public static InventoryManager Inven => Instance._inventory;

    public static WebManager Web => Instance._web;

    #endregion Contents

    #region Core

    private DataManager _data = new DataManager();
    private PoolManager _pool = new PoolManager();
    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();

    public static DataManager Data => Instance._data;
    public static PoolManager Pool => Instance._pool;
    public static ResourceManager Resource => Instance._resource;
    public static SceneManagerEx Scene => Instance._scene;
    public static SoundManager Sound => Instance._sound;
    public static UIManager UI => Instance._ui;

    #endregion Core

    public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
    }

    private static void Init()
    {
        // Logger 등록
        GlobalLogger.WriteLog += log => Debug.Log(log);

        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._data.Init();
            s_instance._pool.Init();
            s_instance._sound.Init();
            s_instance._network.Init();
        }
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        _network.Update();
    }
}
