using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Carousel.BaselineAgent;
using Mirror;
using UnityEngine.SceneManagement;

public class MirrorGameManager : MonoBehaviour
{
    public static MirrorGameManager Instance;
    public DebugMessage debugMessage;
    public UserMenu userMenu;
    public ClientConfig config;
    public RPMUserAvatar player;


    public bool host;
    public bool server;
    public bool client;
    
    public string ClientSceneName;
    public string ServerSceneName;
    public bool loadAdditiveScene = true;

    void Start()
    {
        if(Instance == null){
            Instance = this;
            LoadConfig();
            StartMirror();
        }else{
            GameObject.DestroyImmediate(this); //singleton monobehavior
        }
    }
    void StartMirror()
    {
        var n = GetComponent<AppNetworkManager>();
        if(client){
            n.StartClient();
            if(loadAdditiveScene)SceneManager.LoadScene(ClientSceneName, LoadSceneMode.Additive);
        }
        if(host){
            n.StartHost();
            if(loadAdditiveScene)SceneManager.LoadScene(ClientSceneName, LoadSceneMode.Additive);
        }
        if(server){
            n.StartServer();
           if(loadAdditiveScene)SceneManager.LoadScene(ServerSceneName, LoadSceneMode.Additive);
            
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

}
