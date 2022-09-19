using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Carousel.BaselineAgent;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Events;
using kcp2k;
using Carousel;

[Serializable]
public struct NetworkManagerPrefabs{
    public GameObject kcp;
    public GameObject telepathy;

}

public delegate void SyncMusic();

public class MirrorGameManager : MonoBehaviour
{
    public static MirrorGameManager Instance;
    public DebugMessage debugMessage;
    public UserMenu userMenu;
    public ClientConfig config;
    public RPMUserAvatar player;
    public NetworkManagerPrefabs networkManagerPrefabs;


    public bool host;
    public bool server;
    public bool client;

    public UnityEvent onStart;
    public UnityEvent onStop;

    public bool registerServerOnline = true;
    public LoadingScreen loadingScreen;
    public SceneLoader sceneLoader;
    public bool loadConfigFromStreamAssets = false;
    public ServerRegistry serverRegistry;
    public SyncMusic SyncMusicCallback;
    public Transform vrRigOrigin;

    void Awake(){
        
        if(Instance == null){
            Instance = this;
        }else{
            GameObject.DestroyImmediate(gameObject); //singleton monobehavior
        }
        
    }


    void Start()
    {

        serverRegistry =gameObject.AddComponent<ServerRegistry>();
        sceneLoader = gameObject.AddComponent<SceneLoader>();
        var assetDatabaseText = Resources.Load<TextAsset>("assetDatabase").text;
        if (assetDatabaseText != null){
            sceneLoader.assetDB = AssetDatabase.Init(assetDatabaseText);
        }else{
            sceneLoader.assetDB = new AssetDatabase();
        }
        sceneLoader.mainSceneLoaded.AddListener(OnMainSceneLoaded);
        sceneLoader.LoadLobbyScene();
        LoadConfig();
        ConfigureTrackers();
        DontDestroyOnLoad(gameObject);
    }

    void ConfigureTrackers()
    {
        var trackerConfig = Camera.main.GetComponent<VRRigConfig>();
        if (trackerConfig == null)
        {
            return;
        }
        ConfigureTracker(trackerConfig.hipTrackerTarget, config.hipTracker);
        ConfigureTracker(trackerConfig.leftFootTarget, config.leftFootTracker);
        ConfigureTracker(trackerConfig.rightFootTarget, config.rightFootTracker);
        ConfigureTracker(trackerConfig.leftControllerTarget, config.leftControllerTracker);
        ConfigureTracker(trackerConfig.rightControllerTarget, config.rightControllerTracker);
    }
    

    public void ConfigureTracker(TrackerTarget tracker, TrackerConfig trackerConfig)
    {
        var _offset = trackerConfig.posOffset;
        tracker.offset = new Vector3(_offset[0], _offset[1], _offset[2]);
        var _rotOffset = trackerConfig.rotOffset;
        tracker.rotationOffset = new Vector3(_rotOffset[0], _rotOffset[1], _rotOffset[2]);
        
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        ShowMessage(logString, false);

    }

    void OnDestroy(){
    }

    public void StartMirror()
    {
        
        GameObject networkManager = networkManagerPrefabs.kcp;
        if(config.protocol == "telepathy"){
            networkManager = networkManagerPrefabs.telepathy;
        }
        GameObject.Instantiate(networkManager);
        onStart?.Invoke();
        var n = AppNetworkManager.singleton;
        if(client){
            ShowMessage("StartClient");
            n.networkAddress = config.url;
            n.StartClient();
        }else if(host){
            ShowMessage("StartHost");
            n.StartHost();
        }else if(server){
            ShowMessage("server");
            n.StartServer();
        }
    }
    public void StopMirror()
    {
        onStop?.Invoke();
        var n = AppNetworkManager.singleton;
        if(client){
            n.StopClient();
        }else if(host){
            n.StopHost();
        }else if(server){
            n.StopServer();
        }
        Destroy(n.gameObject);
    }
    
    protected void LoadConfig()
    {
      
        config = ClientConfig.GetInstance();
        if(config == null){
            string configText = "";
            if(loadConfigFromStreamAssets){
                string configFile = Path.Combine(Application.streamingAssetsPath, "config.json");
                configText = File.ReadAllText(configFile);
            }else{
                configText = Resources.Load<TextAsset>("config").text;
            }
            config = ClientConfig.InitInstance(configText);
            serverRegistry.SetConfig(config);
            registerServerOnline = config.registerServerOnline;
            ShowMessage("Mirror Game Manager: loaded config "+configText);
        }
        

    }

    public static void ShowMessage(string message, bool print=false){
        if(print)Debug.Log("debug: "+message);
        if(Instance != null && Instance.debugMessage != null){
            Instance.debugMessage.Write(message);
        }
    }

    public static void ToggleConsole(){
        if(Instance != null && Instance.debugMessage != null){
            Instance.debugMessage.ToggleVisibility();
        }

    }

    public void RegisterPlayer(RPMUserAvatar player){
        this.player = player;
        if(userMenu != null){
            userMenu.Register(player);
        }
    }

    public void HostServer(){
        
        this.host = true;
        this.client = false;
        if(registerServerOnline)RegisterServer();
        sceneLoader.LoadMainScene();
    }



    public void RegisterServer(){
        serverRegistry.RegisterServer();
    }

    public void UnregisterServer(){
        serverRegistry.UnregisterServer();
    }

            
    public void JoinServer(string url, string protocol="", int port=-1)
    {
        this.host = false;
        this.client = true;
        if (url != "") config.url = url;
        if (protocol != "") config.protocol = protocol;
        if (port >-1) config.port = port;
        ConfigureTrackers();
        sceneLoader.LoadMainScene();
    }

    public void StartServer()
    {
        this.server = true;
        if(registerServerOnline)RegisterServer();
        ConfigureTrackers();
        sceneLoader.LoadMainScene();
    }


    public void OpenMainMenu()
    {
        var trackerConfig = Camera.main.GetComponent<VRRigConfig>();
        if(trackerConfig !=null)trackerConfig.DisableAllTargets();
        ShowMessage("ToMainMenu ");
        if (registerServerOnline && (host || server))UnregisterServer();
        StopMirror();
        this.host = false;
        this.server = false;
        this.client = false;
        sceneLoader.LoadLobbyScene();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void OnMainSceneLoaded()
    {
        StartMirror();
    }

    public IEnumerator ShowLoadingScreen(string info, AsyncOperation operation){
        if(loadingScreen != null){
            yield return loadingScreen.Show(info, operation);
        }else{
            yield return null;
        }
    }


    public void GrabLeftHand(){
        if (player== null) return;
        player.commands.GrabLeftHand();
        player.handAnimationController.CloseLeftHand();

    }

    public void GrabRightHand(){
        if (player== null) return;
        player.commands.GrabRightHand();
        player.handAnimationController.CloseRightHand();
        
    }

    public void ReleaseLeftHand(){
        if (player== null) return;
        player.commands.ReleaseLeftHand();
        player.handAnimationController.OpenLeftHand();

    }

    public void ReleaseRightHand(){
        if (player== null) return;
        player.commands.ReleaseRightHand();
        player.handAnimationController.OpenRightHand();
        
    }

    public void SyncMusic(){

        if(SyncMusicCallback!=null)SyncMusicCallback();
    }
    

}
