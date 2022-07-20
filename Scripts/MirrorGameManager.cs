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

public class MirrorGameManager : MonoBehaviour
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
            Instance.debugMessage.Show(message);
        }
    }

    public void RegisterPlayer(RPMUserAvatar player){
        this.player = player;
        if(userMenu != null){
            userMenu.Register(player);
        }
    }

      
    public void EnterScene(bool host=false)
    {
        this.host = host;
        this.client = !host;
        StartCoroutine(LoadYourAsyncScene());
    }
      
    public void StartServer()
    {
        this.server = true;
        StartCoroutine(LoadYourAsyncScene());
    }

    public void ExitGame()
    {
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



}
