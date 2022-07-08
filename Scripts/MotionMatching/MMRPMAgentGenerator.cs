using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadyPlayerMe;
using Carousel.BaselineAgent;
using Carousel.FigureGenerator;
using UnityEngine.AI;
using Mirror;
using Carousel.MotionMatching;


namespace Carousel{
    
public class MMRPMAgentGenerator : RPMAgentGenerator
{
    public string mmFilename;
    public MMSettings mmSettings;
    public MMControllerSettigs mmControllerSettings;
    public List<MMRuntimeRetargetingV1.RetargetingMap> retargetingMap;
    
    override public void CreateServerAgentController(GameObject avatar){
        var prefab = GameObject.Instantiate(avatar);
        var ragdollGenerator = gameObject.AddComponent<PDRagDollGenerator>();
        ragdollGenerator.runOnAwake = false;
        ragdollGenerator.character = avatar;
        ragdollGenerator.CharacterPrefab = prefab;
        ragdollGenerator.settings = settings;
        ragdollGenerator.animationController = animationController;
        ragdollGenerator.IgnoreList = IgnoreList;
        ragdollGenerator.useAvatarModel = useAvatarModel;
        ragdollGenerator.createAnimationSource = createAnimationSource;
        ragdollGenerator.createStabilizerJoint = createStabilizerJoint;
        ragdollGenerator.createMirror = createMirror;
        ragdollGenerator.mirrorSettings = mirrorSettings;
      
        ragdollGenerator.stabilizerJointPrefab = stabilizerJointPrefab;
        ragdollGenerator.hideReference = hideReference;
        ragdollGenerator.Generate();

        PhysicsPoseProvider poseProvider = GetComponentInChildren<PhysicsPoseProvider>();
        poseProvider.armatureName = armatureName;

        NetworkAgentController ac = ConfigurePoseProvider(poseProvider.gameObject);
        
        //AddAgentInteraction(go, ac);
        DestroyImmediate(prefab);
    }

    override public NetworkAgentController ConfigurePoseProvider(GameObject poseProvider){
        var ac = GetComponent<MMNetworkAgentController>();
        var mm = poseProvider.AddComponent<MotionMatching.MotionMatching>();
        mm.filename = mmFilename;
        mm.settings = mmSettings;
        var controller = poseProvider.AddComponent<TargetLocomotionController>();
        Transform root;
        GeneratorUtils.FindChild(poseProvider.transform, settings.rootName, out root);
        controller.root = root;
        controller.mm = mm;
        controller.settings = mmControllerSettings;

        var retargeting = poseProvider.AddComponent<MMRuntimeRetargetingV1>();
        retargeting.src = controller;
        retargeting.retargetingMap = retargetingMap;

        // add compositor that combines input from motion matching retargeting and mirroring
        var compositor= poseProvider.AddComponent<PoseCompositor>();
        compositor.posers = new List<CharacterPoser>();
        compositor.Add(retargeting);
        var mirror= poseProvider.GetComponent<RuntimeMirroring>();
        if (mirror != null) compositor.Add(mirror);
        ac.poseCompositor = compositor;

        ac.locomotionController = controller;
        ac.minStartDistance = minStartDistance;
        ac.minStopDistance = minStopDistance;
        ac.mirror = poseProvider.GetComponent<RuntimeMirroring>();
        ac.pdController = GetComponentInChildren<RagDollPDController>();
        ac.follower = GetComponentInChildren<PhysicsPairDanceFollower>();
        ac.pdController.alignReferenceRoot = false;
        ac.pdController.createRootJoint = true;
        ac.pdController.mode = PDControllerMode.OFF;
        ac.pdController.delayedActivation = true;
        AddNetworkAgentInteraction(poseProvider.transform, ac);
        return ac;
    }
}
}