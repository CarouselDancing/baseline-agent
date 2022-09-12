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
   
    public static int LEFT = 0;
    public static int RIGHT = 1;
    public static float HEIGHT_STEP_SIZE = 0.05f;
    public FigureGeneratorSettings settings;
    public int figureVersion;
    public GameObject PlayerInteractionZonePrefab;
    public GameObject PartnerTargetPrefab;
    public GameObject UserTriggerPrefab;
    public RoomConfig roomConfig;
    public ClientConfig clientConfig;
    public GameObject generator;
    public Vector3 interactionZoneOffset;
    public PlayerInteractionZone interactionZone;
    public float grabberTriggerRadius = 0.1f;
    public CharacterRigConfig rigConfig;
    public UnityEvent OnFinished;
    public RBGrabber leftGrabber;
    public RBGrabber rightGrabber;
    public bool activateIK = true;
    public UserAvatarCommands commands;
    public HandAnimationController handAnimationController;
    public UserConnectionVisualization userConnectionVis;
    public bool isObserver = false;

    override public void Start()
    {
        MirrorGameManager.ShowMessage("Created Avatar");
        roomConfig = (RoomConfig)GameObject.FindObjectsOfTypeAll(typeof(RoomConfig)).FirstOrDefault();
        clientConfig = MirrorGameManager.Instance.config;
        networkAvatar = GetComponent<NetworkAvatar>();
        commands = GetComponent<UserAvatarCommands>();
        if (IsOwner)
        {
            MirrorGameManager.ShowMessage(" GlobalGameState.GetInstance");
            isObserver= MirrorGameManager.Instance.config.isObserver;
            if (isObserver){
                CmdSetObserver(isObserver);
                return;
            }
            var avatarIndex = clientConfig.userAvatar;
            AvatarURL = clientConfig.rpmAvatars[avatarIndex].url;
            
            MirrorGameManager.ShowMessage("SetupAvatarControllerFromRPM"+AvatarURL);
            if (AvatarURL != "")
            {

                SetupAvatarControllerFromRPM(AvatarURL);
                CmdSetURL(AvatarURL);
            }
            else
            {
                Debug.Log("Error: avatar url is emtpy");
            }
        }else{
            
            MirrorGameManager.ShowMessage("Do nothing");
        }

    }

   
    override public void OnRPMAvatarLoaded(GameObject avatar, AvatarMetaData metaData=null)
    {
        Debug.Log("Load user avatar");
        MirrorGameManager.ShowMessage("OnRPMAvatarLoaded");
        bool activateFootRig = clientConfig.activateFootTrackers;
        var ikRigBuilder = new RPMIKRigBuilder(animationController, activateFootRig, true);
        ikRigBuilder.handCenterOffset = settings.handCenterOffset;
        rigConfig = ikRigBuilder.Build(avatar, IsOwner);
        SetupRig(rigConfig, avatar);
        CreateRigidBodyFigure(avatar, rigConfig.Root, settings.modelLayer);
        var root = rigConfig.Root;
        var pli = Instantiate(PlayerInteractionZonePrefab);
        pli.transform.parent = root;
        pli.transform.localPosition = interactionZoneOffset;
        interactionZone = pli.GetComponent<PlayerInteractionZone>();
        var pto = Instantiate(PartnerTargetPrefab);
        pto.transform.parent = avatar.transform;
        var pt = pto.GetComponent<PartnerTarget>();
        pt.leader = root;
        pt.offset = interactionZoneOffset;
        var controller = avatar.AddComponent<PlayerControllerBase>();
        controller.root = root;
        interactionZone.player = controller;
        interactionZone.partnerTarget = pto.transform;


        var userTrigger = Instantiate(UserTriggerPrefab);
        userTrigger.transform.parent = root;
        userTrigger.transform.localPosition = Vector3.zero;
        userTrigger.GetComponent<UserTrigger>().playerInteraction = interactionZone;

        //store mirrored targets for hand holding
        interactionZone.ikTargets = new Dictionary<int, Transform>();
        interactionZone.ikTargets[(int)RBGrabber.Side.RIGHT] = rigConfig.LeftHandCenter;
        interactionZone.ikTargets[(int)RBGrabber.Side.LEFT] = rigConfig.RightHandCenter;


        leftGrabber = AddGrabber(rigConfig.LeftHand.gameObject,  RBGrabber.Side.LEFT);
        rightGrabber = AddGrabber(rigConfig.RightHand.gameObject, RBGrabber.Side.RIGHT);
        
        handAnimationController = avatar.AddComponent<HandAnimationController>();
        if(isLocalPlayer)MirrorGameManager.Instance.RegisterPlayer(this);
        MirrorGameManager.ShowMessage($"Avatar loaded. [{Time.timeSinceLevelLoad:F2}]\n\n");
        userConnectionVis = avatar.AddComponent<UserConnectionVisualization>();
        userConnectionVis.Init(rigConfig.LeftHand, rigConfig.RightHand);
        OnFinished?.Invoke();
      
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
        float avatarHeight = rigConfig.Head.position.y - rigConfig.ToeTip.position.y;
        float yOffset = avatarHeight - Camera.main.transform.position.y;
        var p = trackerConfig.origin.position;
        p.y += yOffset;
        trackerConfig.origin.position = p;
        MirrorGameManager.ShowMessage("SetHeight "+yOffset.ToString());
      
   }
   

    public void MoveUp(){
        
        var trackerConfig = Camera.main.GetComponent<VRRigConfig>();
        if(trackerConfig== null){
            return;

        }
        var p = trackerConfig.origin.position;
        p.y += HEIGHT_STEP_SIZE;
        trackerConfig.origin.position = p;
    }

    public void MoveDown(){
        var trackerConfig = Camera.main.GetComponent<VRRigConfig>();
        if(trackerConfig== null){
            return;

        }
        var p = trackerConfig.origin.position;
        p.y -= HEIGHT_STEP_SIZE;
        trackerConfig.origin.position = p;
    }

    //handle object interaction based on grabbing

    public void GrabHand(int side){
        if (side == RIGHT){
             //connnect right user hand to left hand of character
            Transform t = null;
            if (activateIK)t=interactionZone.ActivateHandIK(LEFT);
            if(t!=null){
                rightGrabber.WaitForObject(t);
            }else{
                rightGrabber.GrabObject();
            }
        }else{
            //connnect left user hand to right hand of character
            Transform t = null;
            if (activateIK)t = interactionZone.ActivateHandIK(RIGHT);
            if(t!=null){
                leftGrabber.WaitForObject(t);
            }else{
                leftGrabber.GrabObject();
            }
        }
    }

    public void ReleaseHand(int side){
        if (side == RIGHT){
            interactionZone.DeactivateHandIK(LEFT);
            rightGrabber.ReleaseObject();
            
        }else{
            interactionZone.DeactivateHandIK(RIGHT);
            leftGrabber.ReleaseObject();
        }
    }

    public void ConnectToOtherPlayer(){
    
        if(interactionZone.otherPlayer == null)return;
        RPMUserAvatar otherPlayerAvatar = interactionZone.otherPlayer.transform.parent.GetComponent<RPMUserAvatar>();
        if(otherPlayerAvatar == null)return;
        var leftTarget = otherPlayerAvatar.leftGrabber.transform;
        var rightTarget = otherPlayerAvatar.rightGrabber.transform;
        userConnectionVis.Activate(leftTarget, rightTarget);

        //tell server to tell all clients to let this player connect to me
        //if(IsOwner)otherPlayerAvatar.commands.CmdConnectToOtherPlayer();

    }

    public void DisconnectFromOtherPlayer(){ 
      
        if(interactionZone.otherPlayer == null)return;
        RPMUserAvatar otherPlayerAvatar = interactionZone.otherPlayer.transform.parent.GetComponent<RPMUserAvatar>();
        if(otherPlayerAvatar == null)return;
        //tell server to tell all clients to let this player diconnect from me
        //if(IsOwner)otherPlayerAvatar.commands.CmdDisconnectFromOtherPlayer();
        userConnectionVis.Deactivate();
    }


    [Command]
    public void CmdSetObserver(bool newValue)
    {
        Debug.Log("CmdSetObserver");
        isObserver = newValue;
        SetObserverOnClient(newValue);
    }

    [ClientRpc]
    void SetObserverOnClient(bool newValue)
    {
        if(!IsOwner) isObserver = newValue;
    }


}

}

}