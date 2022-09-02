using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;
using Carousel.BaselineAgent;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Events;
using kcp2k;

[Serializable]
public struct NetworkManagerPrefabs{
    public GameObject kcp;
    public GameObject telepathy;

}

public struct ServerEntry{
    public string name;
    public string address;
    public int port;
    public string protocol;

}


public class MirrorGameManager : RESTInterface
{
    public GameObject loadingscreen;
    public Slider slider;
    public static MirrorGameManager Instance;
    public DebugMessage debugMessage;
    public UserMenu userMenu;
    public ClientConfig config;
    public RPMUserAvatar player;
    public NetworkManagerPrefabs networkManagerPrefabs;


    public bool host;
    public bool server;
    public bool client;
    
    
    public string lobbyScene = "Lobby";
    public string mainScene = "main";
    public bool loadAsync;

    public UnityEvent onStart;
    public UnityEvent onStop;

    public bool registerServerOnline = true;

    void Awake(){
        
        if(Instance == null){
            Instance = this;
        }else{
            GameObject.DestroyImmediate(gameObject); //singleton monobehavior
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void Start()
    {

        if(loadAsync){
            StartCoroutine(LoadYourAsyncScene(lobbyScene));
        }else{    
            SceneManager.LoadScene(lobbyScene);
        }
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
            // string configFile = Path.Combine(Application.streamingAssetsPath, "config.json");
                var configText = Resources.Load<TextAsset>("config").text;
            // string configText = File.ReadAllText(configFile);
            config = ClientConfig.InitInstance(configText);
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
        if(loadAsync){
            StartCoroutine(LoadYourAsyncScene(mainScene));
        }else{    
            SceneManager.LoadScene(mainScene);
        }
    }
    public ServerEntry getServerEntry(){
        var url = LocalIPAddress();
        var serverEntry = new ServerEntry(){
            name = url,
            address = url,
            port = config.port,
            protocol = config.protocol
        };
        return serverEntry;
    }

    public void RegisterServer(){
        Debug.Log("register server");
        var serverEntry = getServerEntry();
        string data = "";
        var setting = new JsonSerializerSettings();
        data = JsonConvert.SerializeObject(serverEntry, setting);
        StartCoroutine(sendRequestCoroutine("dance_servers/add", data, PrintResponse));
    }

    public void UnregisterServer(){
        var serverEntry = getServerEntry();
        string data = "";
        var setting = new JsonSerializerSettings();
        data = JsonConvert.SerializeObject(serverEntry, setting);
        StartCoroutine(sendRequestCoroutine("dance_servers/remove", data, PrintResponse));
    }

    public void PrintResponse(string responseText){
        Console.WriteLine(responseText);
    }
            
    public void JoinServer(string url, string protocol="", int port=-1)
    {
        this.host = false;
        this.client = true;
        if (url != "") config.url = url;
        if (protocol != "") config.protocol = protocol;
        if (port >-1) config.port = port;
        ConfigureTrackers();
        if(loadAsync){
            StartCoroutine(LoadYourAsyncScene(mainScene));
        }else{    
            SceneManager.LoadScene(mainScene);
        }
    }

    public void StartServer()
    {
        this.server = true;
        if(registerServerOnline)RegisterServer();
        ConfigureTrackers();
        if(loadAsync){
            StartCoroutine(LoadYourAsyncScene(mainScene));
        }else{    
            SceneManager.LoadScene(mainScene);
        }
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
        if(loadAsync){
            StartCoroutine(LoadYourAsyncScene(lobbyScene));
        }else{
            SceneManager.LoadScene(lobbyScene);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
    IEnumerator LoadYourAsyncScene(string s)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(s);
        if(loadingscreen != null) loadingscreen.SetActive(true);
        // asyncLoad.allowSceneActivation = false;
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            float loading = Mathf.Clamp01(asyncLoad.progress / .9f);
            if(slider != null) slider.value = loading;
            yield return null;
        } 
        if(loadingscreen != null) loadingscreen.SetActive(false);
        
    }


    //https://answers.unity.com/questions/1731994/get-the-device-ip-address-from-unity.html
    public static string LocalIPAddress()
     {
         return Dns.GetHostEntry(Dns.GetHostName())
             .AddressList.First(
                 f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
             .ToString();
     }    



    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ShowMessage("OnSceneLoaded "+ scene.name);
        if(scene.name == mainScene){
            StartMirror();
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
    


}
