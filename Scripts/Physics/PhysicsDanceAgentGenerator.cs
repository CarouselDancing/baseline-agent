using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Carousel.FigureGenerator;

namespace Carousel{
    
namespace BaselineAgent{



[RequireComponent(typeof(FigureGeneratorSettings))]
public class PhysicsDanceAgentGenerator : PDRagDollGenerator
{
    public bool createReferencePose;
    public bool createFollowerScript;
    GameObject referencePose;
    public bool createRootJoint;

    override public void Generate()
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
        if(createReferencePose)CreateReferencePoseTarget();
        AddPDControllerComponents();
        if(createMirror)AddMirrorComponent(animationSource);
        transform.position = originalPos;
        transform.rotation = originalRot;
        OnFinished?.Invoke();
        if (DestroyWhenDone) DestroyImmediate(this);
    }



   
    override public void AddPDControllerComponents()
    {

        if (createAnimationSource) CreateAnimationSource();
        
        var ragoll = character.AddComponent<RagDoll003>();
        ragoll.MusclePowers = settings.MusclePowers;
        ragoll.ignoreList = settings.ignoreList;


        RagDollPDControllerBase pdController;
        if (mode == GeneratorMode.ArticulationBody || mode == GeneratorMode.Prefab){
            pdController = character.AddComponent<RagDollPDController>();
        }else{
            pdController = character.AddComponent<RBRagDollPDController>();
        }
        pdController.upperBodyNames = settings.upperBodyNames;
        pdController.createRootJoint = createRootJoint;
        pdController.alignReferenceRoot = false;
        pdController.DebugPauseOnReset = DebugPauseOnReset;
        if (createAnimationSource) { 
           pdController.animationSrc = animationSource.GetComponent<PhysicsPoseProvider>();
           pdController.animationSrc.stabilizer = stabilizerJoint;
        }
        if(createFollowerScript && createMirror){

            var follower =  character.AddComponent<PhysicsPairDanceFollower>();
            follower.mirror = animationSource.GetComponent<RuntimeMirroring>();
            follower.root = rootArticulationBody;
            follower.mode = PhysicsPairDanceFollower.FollowMode.JOINT;
        }
        
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