using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Carousel.FigureGenerator;

namespace Carousel{
    
namespace BaselineAgent{

public enum GeneratorMode{
    ArticulationBody,
    Rigidbody,
    Prefab
}


[RequireComponent(typeof(FigureGeneratorSettings))]
public class PhyscisDanceAgentGenerator : MonoBehaviour
{

    FigureGeneratorSettings settings;


    public GameObject CharacterPrefab;
    public GameObject character;
    private GameObject animationSource;
    private GameObject referencePose;
    public GameObject stabilizerJointPrefab;
    public GameObject agentInteractionPrefab;

    public bool DebugPauseOnReset = false;
    public bool useAvatarModel = false;
    public bool DestroyWhenDone = true;
    public MirrorSettings mirrorSettings;
    public bool runOnAwake = true;
    public UnityEvent OnFinished;
    public bool createAnimationSource;    
    public bool createMirror;    
    public bool createReferencePose;
    public bool createStabilizerJoint;
    public bool CreateFollowerScript;
    public int version = 2;

    public AnimatorOverrideController animationController;
    ConfigurableJoint stabilizerJoint;
    ArticulationBody rootArticulationBody;
    Rigidbody rootRigidBody;

    public GeneratorMode mode;

    public bool hideReference;


    void Awake()
    {

        if(runOnAwake)Generate();
    }

    public void StartGenerate()
    {
        StartCoroutine(GenerateCoroutine());
    }

    public IEnumerator GenerateCoroutine()
    {
        Generate();
        yield return null;
    }


    public void Generate()
    {
        settings = GetComponent<FigureGeneratorSettings>();
     

        var originalPos = transform.position;
        var originalRot = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        if (useAvatarModel)
        {
            HumanoidSettingsGenerator.CreateSettingsFromHumanoidModel(character, ref settings);
        }

        character.GetComponent<Animator>().enabled = false;
        if (mode == GeneratorMode.ArticulationBody){
            rootArticulationBody = CreateArticulationBodyFigure(character, settings.modelLayer);
        }else{
            rootRigidBody = CreateRigidBodyFigure(character, settings.modelLayer, false, false);
        }

        if (createStabilizerJoint && stabilizerJointPrefab != null){
            var jointO = GameObject.Instantiate(stabilizerJointPrefab);
            if (settings.modelLayer != "") GeneratorUtils.SetLayerRecursively(jointO.transform, LayerMask.NameToLayer(settings.modelLayer));
            jointO.transform.position = Vector3.zero;
            jointO.transform.parent = transform;
            stabilizerJoint = jointO.GetComponent<ConfigurableJoint>();

                if (mode == GeneratorMode.ArticulationBody)stabilizerJoint.connectedArticulationBody = rootArticulationBody;
                else if (mode == GeneratorMode.Rigidbody)stabilizerJoint.connectedBody = rootRigidBody;
        }
        if (createAnimationSource) CreateAnimationSource();
        if(createReferencePose)CreateReferencePoseTarget();
        AddEnvironmentComponents();

        transform.position = originalPos;
        transform.rotation = originalRot;
        OnFinished?.Invoke();
        if (DestroyWhenDone) DestroyImmediate(this);
    }
    ArticulationBody CreateArticulationBodyFigure(GameObject o, string layer)
    {
        var abGenerator = o.AddComponent<ArticulationBodyFigureGenerator>();
        abGenerator.width = settings.width;
        abGenerator.mat = settings.mat;
        abGenerator.IgnoreList = new List<string>();
        abGenerator.leftFoot = settings.leftFoot;
        abGenerator.rightFoot = settings.rightFoot;
        abGenerator.headPrefab = settings.headPrefab;
        abGenerator.reference = settings.reference;
        abGenerator.footOffset = settings.footOffset;
        abGenerator.lengthScaleFactor = settings.lengthScaleFactor;
        abGenerator.headOffset = settings.headOffset;
        abGenerator.alignAnchorRotation = settings.alignAnchorRotation;
        abGenerator.createColliderAsChild = settings.createColliderAsChild;
        abGenerator.version = version;
        abGenerator.figureType = settings.figureType;
        abGenerator.disableLimits = settings.disableLimits;
        var root = o.transform.Find(settings.rootName);
        abGenerator.root = root;
        abGenerator.referenceBodies = new List<ArticulationBodyFigureGenerator.RefBodyMapping>();
        foreach (var r in settings.referenceBodies)
        {
            var _r = new ArticulationBodyFigureGenerator.RefBodyMapping { name = r.name, refName = r.refName };
            abGenerator.referenceBodies.Add(_r);
        }
        abGenerator.endEffectors = new List<string>();
        foreach (var n in settings.endEffectors)
        {
            abGenerator.endEffectors.Add(n);
        }

        abGenerator.DestroyWhenDone = DestroyWhenDone;
        abGenerator.Generate();
        GeneratorUtils.SetLayerRecursively(character.transform, LayerMask.NameToLayer(layer));
        return root.GetComponent<ArticulationBody>();
    }


    Rigidbody CreateRigidBodyFigure(GameObject o, string layer, bool isKinematic=true, bool hide=false)
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
        rbGenerator.root = o.transform.Find(settings.rootName);
        rbGenerator.version = version;
        rbGenerator.figureType = settings.figureType;
        rbGenerator.isKinematic = isKinematic;
        rbGenerator.referenceBodies = new List<RigidBodyFigureGenerator.RefBodyMapping>();
        var root = o.transform.Find(settings.rootName);
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
        rbGenerator.DestroyWhenDone = DestroyWhenDone;
        rbGenerator.verbose = false;
        rbGenerator.Generate();
        if(hide) {
            SetMeshVisiblity(o.transform, false);
            RemoveAnimatedMeshMaterial(o.transform);
        }
        if (layer != "") GeneratorUtils.SetLayerRecursively(o.transform, LayerMask.NameToLayer(layer));
         return root.GetComponent<Rigidbody>();
    }

    public void SetMeshVisiblity(Transform target, bool visible){
        
        var meshes = target.GetComponentsInChildren<MeshRenderer>().ToList();
        foreach (var mesh in meshes)
        {
            mesh.enabled = visible;
        }
    }

    public void RemoveAnimatedMeshMaterial(Transform target){
        
        var skinnedMeshes = target.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
        foreach (var mesh in skinnedMeshes)
        {
            mesh.materials = new List<Material>().ToArray();
        }
    }



    //add components order matters: agent and decision requester must be at the end
    void AddEnvironmentComponents()
    {

        var ragoll = character.AddComponent<RagDoll003>();
        ragoll.MusclePowers = settings.MusclePowers;
        ragoll.ignoreList = settings.ignoreList;


        RagDollPDControllerBase pdController;
        if (mode == GeneratorMode.ArticulationBody || mode == GeneratorMode.Prefab)
            pdController = character.AddComponent<RagDollPDController>();
        else
            pdController = character.AddComponent<RBRagDollPDController>();
        pdController.DebugPauseOnReset = DebugPauseOnReset;
        if (createAnimationSource) { 
           pdController.animationSrc = animationSource.GetComponent<PhysicsPoseProvider>();
           pdController.animationSrc.stabilizer = stabilizerJoint;
        }
        if(CreateFollowerScript && createMirror){

            var follower =  character.AddComponent<PhyscisPairDanceFollower>();
            follower.mirror = animationSource.GetComponent<RuntimeMirroring>();
            follower.root = rootArticulationBody;
            //follower.joint = stabilizerJoint;
            follower.mode = PhyscisPairDanceFollower.FollowMode.JOINT;
            // danceAgent.follower = follower;
            /*var danceAgent = gameObject.AddComponent<PhyscisDanceAgent>();
            danceAgent.mirror = animationSource.GetComponent<RuntimeMirroring>();
            danceAgent.pdController = pdController;
           // danceAgent.referencePose = referencePose.transform;
            var follower =  gameObject.AddComponent<PhyscisPairDanceFollower>();
            follower.reference = referencePose.transform;
            follower.mirror = animationSource.GetComponent<RuntimeMirroring>();
            follower.root = rootArticulationBody;
            //follower.joint = stabilizerJoint;
            follower.mode = PhyscisPairDanceFollower.FollowMode.JOINT;
            danceAgent.follower = follower;
            var agentInteraction = GameObject.Instantiate(agentInteractionPrefab);
            agentInteraction.transform.parent = character.transform;
            agentInteraction.transform.localPosition = Vector3.zero;
            var zone = agentInteraction.GetComponent<AgentInteraction>();
            zone.agent = danceAgent;*/
        }
        
    }    
    
    void CreateAnimationSource()
    {
        animationSource = GameObject.Instantiate(CharacterPrefab);
        
        animationSource.transform.position = character.transform.position;
        animationSource.transform.rotation = character.transform.rotation;
        if (settings.tansluscentMat != null) GeneratorUtils.SetMaterialRecursively(animationSource.transform, settings.tansluscentMat);
        animationSource.name += "PoseProvider";
        Animator anim = animationSource.GetComponent<Animator>();
        anim.runtimeAnimatorController = animationController;
        anim.enabled = true;
        PhysicsPoseProvider poseProvider;
        if(createMirror){
            poseProvider = AddMirrorComponent(animationSource);
        }else{
            poseProvider = animationSource.AddComponent<PhysicsPoseProvider>();
            poseProvider.rootName = settings.rootName;
        }
    
        animationSource.transform.position = character.transform.position;
        poseProvider.transform.parent = transform;
        CreateRigidBodyFigure(animationSource, settings.referenceLayer, true, hideReference);

    }

    PhysicsPoseProvider AddMirrorComponent(GameObject o){
        var mirror = o.AddComponent<RuntimeMirroring>();
        mirror.src = null;
        mirror.mirrorVector = mirrorSettings.mirrorVector;
        mirror.relativeRootOffset = mirrorSettings.mirrorRootOffset;
        mirror.mode = mirrorSettings.mode;
        mirror.translationMode = mirrorSettings.translationMode;
        mirror.rootName = settings.rootName;
        mirror.rootPosSet = false;
        
        if(useAvatarModel)
        {   
            mirror.jointMap = RuntimeMirroring.CreateHumanoidMirrorMap(o);
        
        }else{
            mirror.jointMap =  new List<RuntimeMirroring.JointMap>();
            foreach (var r in mirrorSettings.jointMap)
            {
                var _r = new RuntimeMirroring.JointMap { src = r.name, dst = r.refName };
                mirror.jointMap.Add(_r);
            }
        }
      
        mirror.groundFeet = mirrorSettings.groundFeet;
        if (mirror.groundFeet) { 
            mirror.footTip = mirror.GetComponentsInChildren<Transform>()?.First(x => x.name == mirrorSettings.footTipName);
        }


        mirror.active = false;
        return mirror;
    }
    
    void CreateReferencePoseTarget()
    {
        referencePose = GameObject.Instantiate(CharacterPrefab);
        referencePose.transform.position = character.transform.position;
        referencePose.transform.rotation = character.transform.rotation;
        if (settings.tansluscentMat != null) GeneratorUtils.SetMaterialRecursively(referencePose.transform, settings.tansluscentMat);
        referencePose.name += "ReferencePose";
        referencePose.GetComponent<Animator>().enabled = true;
        referencePose.transform.parent = transform;
        CreateRigidBodyFigure(referencePose, settings.referenceLayer,  true, hideReference);
    }

    

}
}
}