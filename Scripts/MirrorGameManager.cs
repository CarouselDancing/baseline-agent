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


public class MirrorGameManager : RESTInterface
{
    public GameObject loadingscreen;
    public Slider slider;
    public static MirrorGameManager Instance;
    public DebugMessage debugMessage;
    public UserMenu userMenu;
    public ClientConfig config;
    public RPMUserAvatar player;


    public bool host;
    public bool server;
    public bool client;
    
    public string scene = "main";

    void Awake(){
        
        if(Instance == null){
            Instance = this;
        }else{
            GameObject.DestroyImmediate(gameObject); //singleton monobehavior
        }
    }


    void Start()
    {

        LoadConfig();
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy(){
    }

    public void StartMirror()
    {
        var n = AppNetworkManager.singleton;
        if(client){
            n.StartClient();
        }else if(host){
            n.StartHost();
        }else if(server){
            n.StartServer();
        }
    }
    
    protected void LoadConfig()
    {
       // string configFile = Path.Combine(Application.streamingAssetsPath, "config.json");
        var configText = Resources.Load<TextAsset>("config").text;
       // string configText = File.ReadAllText(configFile);
        config = JsonUtility.FromJson<ClientConfig>(configText);
        ShowMessage("loaded config "+configText);
        client = config.networkMode == "client";
        server = config.networkMode == "server";
        host = config.networkMode == "host";

    }

    public static void ShowMessage(string message){
        Debug.Log("debug: "+message);
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
        RegisterServer();
        StartCoroutine(LoadYourAsyncScene());
    }

    public void RegisterServer(){
        Debug.Log("register server");
        var url = LocalIPAddress();
        var serverEntry = new ServerEntry(){
            name = url,
            address = url,
            port = 7777,
            protocol = "kcp"
        };
        string data = "";
        var setting = new JsonSerializerSettings();
        data = JsonConvert.SerializeObject(serverEntry, setting);
        //Action<string> printResponse = (respText) => {Console.WriteLine(respText);};
        StartCoroutine(sendRequestCoroutine("dance_servers/add", data, PrintResponse));
    }

    public void UnregisterServer(){

        var url = LocalIPAddress();
        var serverEntry = new ServerEntry(){
            name = url,
            address = url,
            port = 7777,
            protocol = "kcp"
        };
        string data = "";
        var setting = new JsonSerializerSettings();
        data = JsonConvert.SerializeObject(serverEntry, setting);
        StartCoroutine(sendRequestCoroutine("dance_servers/remove", data, PrintResponse));
    }

    public void PrintResponse(string responseText){
        Console.WriteLine(responseText);
    }
            
    public void JoinServer(string url)
    {
        this.host = false;
        this.client = true;
        if (url != "") config.url = url;
        StartCoroutine(LoadYourAsyncScene());
    }

    public void StartServer()
    {
        this.server = true;
        StartCoroutine(LoadYourAsyncScene());
    }

    public void ExitGame()
    {
        if (host)UnregisterServer();
        Application.Quit();
    }
    
    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
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
        
        StartMirror();
    }


    //https://answers.unity.com/questions/1544275/how-to-get-local-ip-in-unity-201824.html
    public static string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "127.0.0.1";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }



}
