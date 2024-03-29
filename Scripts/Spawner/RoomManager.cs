using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Carousel.BaselineAgent{

public class RoomManager : NetworkBehaviour
{
    public GameObject generator;
    public GameObject cube;
    public RoomConfig roomConfig;
    public static RoomManager Instance;

    public int numAgents;

    public void Awake(){
        if(Instance != null) DestroyImmediate(this); // only allow one instance
        Instance = this;
    }

    public void Start(){
        if(!isServer)return;
        for(int i = 0; i < numAgents;i++){
            SpawnAgent();
        }
        SpawnCube();
    }

    public void SpawnAgent()
    {
        if (roomConfig == null || roomConfig.rpmAvatars.Count == 0 || roomConfig.StartZones.Count == 0 ||  roomConfig.AnimationOverriders.Count==0)  return;
        int mi =  UnityEngine.Random.Range(0, roomConfig.rpmAvatars.Count);
        int ai = UnityEngine.Random.Range(0, roomConfig.AnimationOverriders.Count);
        int si = UnityEngine.Random.Range(0, roomConfig.StartZones.Count);
        string avatarURL =  roomConfig.rpmAvatars[mi].url;
        Vector3 position = roomConfig.StartZones[si].GetRandomStartPosition();
        Quaternion rotation = GetRandomRotation();
        
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
    
    public void SpawnCube()
    {
        var go = GameObject.Instantiate(cube);
        NetworkServer.Spawn(go);
    }

}
}