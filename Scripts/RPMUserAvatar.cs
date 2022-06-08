using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using ReadyPlayerMe;
using Carousel.FigureGenerator;
namespace Carousel{
    namespace BaselineAgent{

public class RPMUserAvatar : RPMAvatarManager
{
   
    public RoomConfig roomConfig;
    public GameObject generator;

    override public void Start()
    {
        roomConfig = GlobalAgentGameState.GetInstance().roomConfig;
        base.Start();

    }

    public void CreateDancer()
    {
        Debug.Log("CreateDancer1");
        if (roomConfig == null || roomConfig.AvatarURLs.Count == 0 || roomConfig.StartZones.Count == 0 || roomConfig.AvatarURLs.Count == 0 || roomConfig.AnimationOverriders.Count==0)  return;
        int mi =  UnityEngine.Random.Range(0, roomConfig.AvatarURLs.Count);
        int ai = UnityEngine.Random.Range(0, roomConfig.AnimationOverriders.Count);
        int si = UnityEngine.Random.Range(0, roomConfig.StartZones.Count);
        string avatarURL =  roomConfig.AvatarURLs[mi];
        Vector3 position = roomConfig.StartZones[si].GetRandomStartPosition();
        Quaternion rotation = GetRandomRotation();
        Debug.Log("CreateDancer2");
        CmdSpawnAgent(avatarURL, position, rotation);
    }

    [Command]
    void CmdSpawnAgent(string avatarURL, Vector3 position, Quaternion rotation)
    {
        if (!NetworkServer.active) return;
        Debug.Log("CmdSpawnAgent");
        var go = GameObject.Instantiate(generator);
        go.transform.position = position;
        go.transform.rotation = rotation;
        var gen = go.GetComponent<RPMAgentGenerator>();
        gen.AvatarURL = avatarURL;
        NetworkServer.Spawn(go);
    }
    
    Quaternion GetRandomRotation(){
        var y =  UnityEngine.Random.Range(-180, 180);
        return Quaternion.Euler(0,y,0);
    }

}

}

}