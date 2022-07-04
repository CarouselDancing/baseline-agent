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
public class PDRagDollGenerator : MonoBehaviour
{

    public FigureGeneratorSettings settings;


    public GameObject CharacterPrefab;
    public GameObject character;
    protected GameObject animationSource;
    public GameObject stabilizerJointPrefab;

    public bool DebugPauseOnReset = false;
    public bool useAvatarModel = false;
    public bool DestroyWhenDone = true;
    public bool runOnAwake = true;
    public UnityEvent OnFinished;
    public bool createAnimationSource;  
    public bool createStabilizerJoint;
    public int version = 2;

    public AnimatorOverrideController animationController;
    protected ConfigurableJoint stabilizerJoint;
    protected  ArticulationBody rootArticulationBody;
    protected  Rigidbody rootRigidBody;

    public GeneratorMode mode;

    public bool hideReference;

    public List<string> IgnoreList;

    void Awake()
    {

        if(runOnAwake && character != null)Generate();
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


    virtual public void Generate()
    {
       if(settings == null) settings = GetComponent<FigureGeneratorSettings>();
     

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
      

        AddPDControllerComponents();

        transform.position = originalPos;
        transform.rotation = originalRot;
        OnFinished?.Invoke();
        if (DestroyWhenDone) DestroyImmediate(this);
    }

    
    public ArticulationBody CreateArticulationBodyFigure(GameObject o, string layer)
    {
        var abGenerator = o.AddComponent<ArticulationBodyFigureGenerator>();
        abGenerator.width = settings.width;
        abGenerator.mat = settings.mat;
        abGenerator.IgnoreList = IgnoreList;
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
        abGenerator.solverIterations = settings.solverIterations;
        Transform root = null;
        GeneratorUtils.FindChild(o.transform, settings.rootName, out root);
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


    public Rigidbody CreateRigidBodyFigure(GameObject o, string layer, bool isKinematic=true, bool hide=false)
    {
        var rbGenerator = o.AddComponent<RigidBodyFigureGenerator>();
        rbGenerator.width = settings.width;
        rbGenerator.mat = settings.mat;
        rbGenerator.IgnoreList = IgnoreList;
        rbGenerator.leftFoot = settings.leftFoot;
        rbGenerator.rightFoot = settings.rightFoot;
        rbGenerator.headPrefab = settings.headPrefab;
        rbGenerator.reference = settings.reference;
        rbGenerator.footOffset = settings.footOffset;
        rbGenerator.lengthScaleFactor = settings.lengthScaleFactor;
        rbGenerator.headOffset = settings.headOffset;
        rbGenerator.createColliderAsChild = settings.createColliderAsChild;
        Transform root = null;
        GeneratorUtils.FindChild(o.transform, settings.rootName, out root);
        rbGenerator.root = root;
        rbGenerator.version = version;
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

    
    virtual public void AddPDControllerComponents()
    {
        if (createAnimationSource) CreateAnimationSource();

        var ragoll = character.AddComponent<RagDoll003>();
        ragoll.MusclePowers = settings.MusclePowers;
        ragoll.ignoreList = settings.ignoreList;


        RagDollPDControllerBase pdController;
        if (mode == GeneratorMode.ArticulationBody || mode == GeneratorMode.Prefab){
            pdController = character.AddComponent<RagDollPDController>();
            pdController.upperBodyNames = settings.upperBodyNames;
        }else{
            pdController = character.AddComponent<RBRagDollPDController>();
        }
        pdController.DebugPauseOnReset = DebugPauseOnReset;
        if (createAnimationSource) { 
           pdController.animationSrc = animationSource.GetComponent<PhysicsPoseProvider>();
           pdController.animationSrc.stabilizer = stabilizerJoint;
        }
        
    }    
    
    public void CreateAnimationSource()
    {
        animationSource = GameObject.Instantiate(CharacterPrefab);
        
        animationSource.transform.position = character.transform.position;
        animationSource.transform.rotation = character.transform.rotation;
        if (settings.tansluscentMat != null) GeneratorUtils.SetMaterialRecursively(animationSource.transform, settings.tansluscentMat);
        animationSource.name += "PoseProvider";
        Animator anim = animationSource.GetComponent<Animator>();
        anim.runtimeAnimatorController = animationController;
        anim.enabled = true;
        animationSource.transform.position = character.transform.position;

        PhysicsPoseProvider poseProvider = animationSource.AddComponent<PhysicsPoseProvider>();
        poseProvider.rootName = settings.rootName;
        poseProvider.transform.parent = transform;

        CreateRigidBodyFigure(animationSource, settings.referenceLayer, true, hideReference);

    }


    

}
}
}