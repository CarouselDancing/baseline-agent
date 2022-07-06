using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadyPlayerMe;
using Carousel.BaselineAgent;
using Carousel.FigureGenerator;
using UnityEngine.AI;
using Mirror;


namespace Carousel{
namespace BaselineAgent{
    
public class RPMAgentGenerator : RPMGeneratorBase
{


    public List<AnimatorOverrideController> animationOverriders;
    public bool runOnStart = true;
    public FigureGeneratorSettings settings;

    public int figureVersion;
    public List<string> IgnoreList;
    public bool useAvatarModel;
    public bool createAnimationSource;    
    public bool createMirror;    
    public bool createReferencePose;
    public bool createStabilizerJoint;
    public bool CreateFollowerScript;
    public GameObject stabilizerJointPrefab;
    public GameObject agentInteractionPrefab;

    public bool hideReference;
    public MirrorSettings mirrorSettings;
    public float minStopDistance = 0.1f;
    public float minStartDistance = 0.5f;
    public string armatureName = "Armature";
    public string rootName = "Hips";
  

    public void Start()
    {
        networkAvatar = GetComponent<NetworkAvatar>();
        if (AvatarURL != "")
        {
            processing = true;
            //if(isServer)SetClientAvatar(AvatarURL); // will only go there in the server
            SetupAvatarControllerFromRPM(AvatarURL);
            if(isServer)syncAvatarURL = AvatarURL;
        }
        else
        {
            //GetAvatarURL();
           Debug.Log("Error: avatar url is emtpy");
        }

    }


    override public void OnRPMAvatarLoaded(GameObject avatar, AvatarMetaData metaData = null)
    {
        if (initiated) {
            DestroyImmediate(avatar);
            return;
        }
        var ikRigBuilder = new RPMIKRigBuilder(animationController, false, false);
        var config = ikRigBuilder.BuildConfig(avatar);

        avatar.transform.position = transform.position;
        avatar.transform.rotation = transform.rotation;
        Debug.Log($"Avatar loaded. [{Time.timeSinceLevelLoad:F2}]\n\n{metaData}");
        initiated = true;
        if (isServer){
            CreateServerAgentController(avatar);
        }else{
            ConfigureClientAgentController(avatar);
        }
        var networkAgent = GetComponent<NetworkAgent>();
        var anim = avatar.GetComponent<Animator>();
        if(anim != null) networkAgent.Init(anim);
    }

    virtual public void CreateServerAgentController(GameObject avatar){
        var prefab = GameObject.Instantiate(avatar);
        var agentGenerator = gameObject.AddComponent<PhysicsDanceAgentGenerator>();
        agentGenerator.runOnAwake = false;
        agentGenerator.character = avatar;
        agentGenerator.CharacterPrefab = prefab;
        agentGenerator.settings = settings;
        agentGenerator.animationController = animationController;
        agentGenerator.IgnoreList = IgnoreList;
        agentGenerator.useAvatarModel = useAvatarModel;
        agentGenerator.createAnimationSource = createAnimationSource;
        agentGenerator.createMirror = createMirror;
        agentGenerator.createReferencePose = createReferencePose;
        agentGenerator.createStabilizerJoint = createStabilizerJoint;
        agentGenerator.createFollowerScript = CreateFollowerScript;
        agentGenerator.stabilizerJointPrefab = stabilizerJointPrefab;
        agentGenerator.hideReference = hideReference;
        agentGenerator.mirrorSettings = mirrorSettings;
        agentGenerator.Generate();

        PhysicsPoseProvider poseProvider = GetComponentInChildren<PhysicsPoseProvider>();
        poseProvider.armatureName = armatureName;

        NetworkAgentController ac = ConfigurePoseProvider(poseProvider.gameObject);
        
        //AddAgentInteraction(go, ac);
        DestroyImmediate(prefab);
    }


    virtual public NetworkAgentController ConfigurePoseProvider(GameObject go){
        //var anim = go.GetComponent<Animator>();
        //anim.runtimeAnimatorController = mainController;
        //go.name = "dancer"+dancers.Count.ToString();
        var ac = GetComponent<AnimatorNetworkAgentController>();
        ac.navMeshAgent = go.AddComponent<NavMeshAgent>();
        ac.navMeshAgent.radius = 0.2f;
        ac.animator = go.GetComponent<Animator>();
        ac.mirror = go.GetComponent<RuntimeMirroring>();
        var asc = go.AddComponent<AnimStateController>();
        asc.animationOverriders = animationOverriders;
        ac.stateController = asc;
        ac.minStartDistance = minStartDistance;
        ac.minStopDistance = minStopDistance;
        ac.ToggleDancing();
        ac.pdController = GetComponentInChildren<RagDollPDController>();
        ac.follower = GetComponentInChildren<PhysicsPairDanceFollower>();
        AddNetworkAgentInteraction(go.transform, ac);
        return ac;
    }
    void ConfigureClientAgentController(GameObject go){
        var ac = GetComponent<NetworkAgentController>();
        var parent = go.transform.Find(armatureName);
        parent = parent.Find(rootName);
        ac.highlight = go.AddComponent<OutlineGroupController>();
        ac.highlight.SetMode(false);
        AddNetworkAgentInteraction(parent, ac);
    }

    public void AddNetworkAgentInteraction(Transform parent, NetworkAgentController ac){
        var io = GameObject.Instantiate(agentInteractionPrefab, parent);
        io.transform.localPosition = Vector3.zero;
        var aic = io.AddComponent<AgentInteraction>();
        aic.agent = ac;
    }
}
}
}