using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using ReadyPlayerMe;
using Carousel.FigureGenerator;
using UnityEngine.Events;


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
    public float grabberTriggerRadius = 0.1f;
    public CharacterRigConfig config;
    public UnityEvent OnFinished;
    public RBGrabber leftGrabber;
    public RBGrabber rightGrabber;

    override public void Start()
    {
        roomConfig = GlobalAgentGameState.GetInstance().roomConfig;
        base.Start();

    }
   
    override public void OnRPMAvatarLoaded(GameObject avatar, AvatarMetaData metaData=null)
    {
        MirrorGameManager.ShowMessage("OnRPMAvatarLoaded");
        bool activateFootRig = GlobalAgentGameState.GetInstance().config.activateFootTrackers;
        var ikRigBuilder = new RPMIKRigBuilder(animationController, activateFootRig);
        config = ikRigBuilder.Build(avatar, IsOwner);
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
        OnFinished?.Invoke();
        leftGrabber = AddGrabber(config.LeftHand.gameObject,  RBGrabber.Side.LEFT);
        rightGrabber = AddGrabber(config.RightHand.gameObject, RBGrabber.Side.RIGHT);
        if(isLocalPlayer)MirrorGameManager.Instance.RegisterPlayer(this);
        Debug.Log($"Avatar loaded. [{Time.timeSinceLevelLoad:F2}]\n\n");
    }
    
    RBGrabber AddGrabber(GameObject o, RBGrabber.Side side){
        var triggerObject = new GameObject(o.name+"trigger");
        triggerObject.transform.position = o.transform.position;
        triggerObject.transform.rotation = o.transform.rotation;
        triggerObject.transform.parent = o.transform;
        var grabber = triggerObject.AddComponent<RBGrabber>();
        grabber.grabberRadius = grabberTriggerRadius;
        grabber.grabber = o.transform.GetComponent<Rigidbody>();//hand rigid body
        grabber.side = side;
        var triggerCollider = triggerObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        return grabber;
    }


    
    public void SpawnAgent()
    {
        CmdSpawnAgent();
    }


    [Command]
    void CmdSpawnAgent()
    {
       RoomManager.Instance.SpawnAgent();
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



    public void SetHeight(){
        var trackerConfig = Camera.main.GetComponent<VRRigConfig>();
        if(trackerConfig== null){
            MirrorGameManager.ShowMessage("SetHeight failed");
            return;

        }
        float avatarHeight = config.Head.position.y - config.ToeTip.position.y;
        float yOffset = avatarHeight - Camera.main.transform.position.y;
        var p = trackerConfig.origin.position;
        p.y += yOffset;
        trackerConfig.origin.position = p;
        MirrorGameManager.ShowMessage("SetHeight "+yOffset.ToString());
      
   }
    public void ToggleFollower(){
        CmdToggleFollower();
    }
    [Command]
    void CmdToggleFollower(){
        interactionZone.ToggleFollower();
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