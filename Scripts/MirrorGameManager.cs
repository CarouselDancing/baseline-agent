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
    public DebugMessage debugMessage;
    public UserMenu userMenu;
    public ClientConfig config;
    public RPMUserAvatar player;

    void Start()
    {
        if(Instance == null){
            Instance = this;
            LoadConfig();
        }else{
            GameObject.DestroyImmediate(this); //singleton monobehavior
        }
    }

    
    protected void LoadConfig()
    {
       // string configFile = Path.Combine(Application.streamingAssetsPath, "config.json");
        var configText = Resources.Load<TextAsset>("config").text;
       // string configText = File.ReadAllText(configFile);
        config = JsonUtility.FromJson<ClientConfig>(configText);
        debugMessage.Show("loaded config "+configText);

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
