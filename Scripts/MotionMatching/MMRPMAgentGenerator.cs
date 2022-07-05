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
        var agentGenerator = gameObject.AddComponent<PDRagDollGenerator>();
        agentGenerator.runOnAwake = false;
        agentGenerator.character = avatar;
        agentGenerator.CharacterPrefab = prefab;
        agentGenerator.settings = settings;
        agentGenerator.animationController = animationController;
        agentGenerator.IgnoreList = IgnoreList;
        agentGenerator.useAvatarModel = useAvatarModel;
        agentGenerator.createAnimationSource = createAnimationSource;
        agentGenerator.createStabilizerJoint = createStabilizerJoint;
      
        agentGenerator.stabilizerJointPrefab = stabilizerJointPrefab;
        agentGenerator.hideReference = hideReference;
        agentGenerator.Generate();

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

        ac.locomotionController = controller;
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