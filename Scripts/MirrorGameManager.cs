using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Carousel.BaselineAgent;
using Mirror;

public class MirrorGameManager : MonoBehaviour
{
    public static MirrorGameManager Instance;
    public RoomConfig roomConfig;
    public DebugMessage debugMessage;
    public NetworkManager networkManager;
    public UserMenu userMenu;
    public ClientConfig config;
    public bool host;
    public bool server;
    public bool client;

    void Start()
    {
        if(Instance == null){
            Instance = this;
            LoadConfig();
        }else{
            GameObject.DestroyImmediate(this); //singleton monobehavior
        }
        if(client)networkManager.StartClient();
        if(host)networkManager.StartHost();
        if(server)networkManager.StartServer();
    }

    
    protected void LoadConfig()
    {
       // string configFile = Path.Combine(Application.streamingAssetsPath, "config.json");
        var configText = Resources.Load<TextAsset>("config").text;
       // string configText = File.ReadAllText(configFile);
        config = JsonUtility.FromJson<ClientConfig>(configText);
        debugMessage.Show("stloadad"+configText);

    }

    public static void ShowMessage(string message){
        if(Instance != null && Instance.debugMessage != null){
            Instance.debugMessage.Show(message);
        }
    }

    public void RegisterPlayer(RPMUserAvatar player){
          if(userMenu != null){
            userMenu.Register(player);
        }
    }

}
