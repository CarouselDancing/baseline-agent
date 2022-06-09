using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using ReadyPlayerMe;
using Carousel.FigureGenerator;


namespace Carousel{
    namespace BaselineAgent{

public class RPMUserAvatar : RPMAvatarManager
{
   
    public FigureGeneratorSettings settings;
    public int figureVersion;
    public GameObject PlayerInteractionZonePrefab;
    public GameObject PartnerTargetPrefab;
    public RoomConfig roomConfig;
    public GameObject generator;
    public Vector3 interactionZoneOffset;
    public PlayerInteractionZone interactionZone;

    override public void Start()
    {
        roomConfig = GlobalAgentGameState.GetInstance().roomConfig;
        base.Start();

    }
   
    override public void OnRPMAvatarLoaded(GameObject avatar, AvatarMetaData metaData=null)
    {
        bool activateFootRig = GlobalGameState.GetInstance().config.activateFootTrackers;
        var ikRigBuilder = new RPMIKRigBuilder(animationController, activateFootRig);
        var config = ikRigBuilder.Build(avatar);
        SetupRig(config, avatar);
        CreateRigidBodyFigure(avatar, config.Root, settings.modelLayer);
        var root = config.Root;
        var pli = Instantiate(PlayerInteractionZonePrefab);
        pli.transform.parent = root;
        pli.transform.localPosition = interactionZoneOffset;
        interactionZone = pli.GetComponent<PlayerInteractionZone>();
        var pt = Instantiate(PartnerTargetPrefab);
        pt.transform.parent = root;
        pt.transform.localPosition = interactionZoneOffset;
        var controller = avatar.AddComponent<PlayerControllerBase>();
        controller.root = root;
        interactionZone.player = controller;
        interactionZone.partnerTarget = pt.transform;
        Debug.Log($"Avatar loaded. [{Time.timeSinceLevelLoad:F2}]\n\n");
    }
    
    public void SpawnAgent()
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

    
     Rigidbody CreateRigidBodyFigure(GameObject o, Transform root, string layer, bool isKinematic=true, bool hide=false)
    {
        var rbGenerator = o.AddComponent<RigidBodyFigureGenerator>();
        rbGenerator.width = settings.width;
        rbGenerator.mat = settings.mat;
        rbGenerator.IgnoreList = new List<string>();
        rbGenerator.leftFoot = settings.leftFoot;
        rbGenerator.rightFoot = settings.rightFoot;
        rbGenerator.headPrefab = settings.headPrefab;
        rbGenerator.reference = settings.reference;
        rbGenerator.footOffset = settings.footOffset;
        rbGenerator.lengthScaleFactor = settings.lengthScaleFactor;
        rbGenerator.headOffset = settings.headOffset;
        rbGenerator.createColliderAsChild = settings.createColliderAsChild;
        rbGenerator.root = root;
        rbGenerator.IgnoreList = new List<string>();
        rbGenerator.version = figureVersion;
        rbGenerator.figureType = settings.figureType;
        rbGenerator.isKinematic = isKinematic;
        rbGenerator.referenceBodies = new List<RigidBodyFigureGenerator.RefBodyMapping>();

        foreach (var r in settings.referenceBodies)
        {
            var _r = new RigidBodyFigureGenerator.RefBodyMapping { name = r.name, refName = r.refName };
            rbGenerator.referenceBodies.Add(_r);
        }
        rbGenerator.endEffectors = new List<string>();
        foreach (var n in settings.endEffectors)
        {
            rbGenerator.endEffectors.Add(n);
        }
        
        //animTarget.AddComponent<CharacterController>();
        rbGenerator.DestroyWhenDone = true;
        rbGenerator.verbose = false;
        rbGenerator.Generate();
        if (layer != "") GeneratorUtils.SetLayerRecursively(o.transform, LayerMask.NameToLayer(layer));
         return root.GetComponent<Rigidbody>();
    }

    
    public void ActivateAgent(){
        CmdActivateAgent();
    }
    [Command]
    void CmdActivateAgent(){
        interactionZone.ActivateAgent();
    }
    public void DeactivateAgent(){
        CmdDeactivateAgent();
    }
    [Command]
    void CmdDeactivateAgent(){
        interactionZone.DeactivateAgent();
    }
    public void ActivateFollower(){
        CmdActivateFollower();
    }

    [Command]
    void CmdActivateFollower(){
        interactionZone.ActivateFollower();
    }
    public void DeactivateFollower(){
        CmdActivateFollower();
    }

    [Command]
    void CmdDeactivateFollower(){
        interactionZone.DeactivateFollower();
        
    } 
    public void ToggleAgentDancing(){
        CmdToggleAgentDancing();
    }

    [Command]
    void CmdToggleAgentDancing(){
        interactionZone.ToggleDancing();
        
    }
    public void ActivatePairDance(){
        CmdActivatePairDance();
    }
    [Command]
    void CmdActivatePairDance(){
        interactionZone.ActivatePairDance();
        
    }
    public void DeactivatePairDance(){
        CmdDeactivatePairDance();
    }
    [Command]
    void CmdDeactivatePairDance(){
        interactionZone.DeactivatePairDance();
        
    }
    public void RemoveAgent(){
        CmdRemoveAgent();
    }
    [Command]
    void CmdRemoveAgent(){
        interactionZone.RemoveAgent();
        
    }

}

}

}