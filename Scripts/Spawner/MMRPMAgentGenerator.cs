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
    public List<MMRuntimeRetargetingV1.RetargetingMap> retargetingMapV1;
    public List<MMRuntimeRetargetingV2.RetargetingMap> retargetingMapV2;
    public bool invertDirection;
    public bool useVelocity;
    public GeneratorMode generatorMode;
    
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
        ragdollGenerator.mode = generatorMode;
        ragdollGenerator.Generate();

        PhysicsPoseProvider poseProvider = GetComponentInChildren<PhysicsPoseProvider>();
        poseProvider.armatureName = armatureName;

        NetworkAgentController ac = ConfigurePoseProvider(poseProvider.gameObject);
        ac.highlight = go.AddComponent<OutlineGroupController>();
        ac.highlight.SetMode(false);
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
        controller.invertDirection = invertDirection;
        controller.useMotionVelocity = useVelocity;
        controller.useMotionAngularVelocity = useVelocity;

        // add compositor that combines input from motion matching retargeting and mirroring
        var compositor= poseProvider.AddComponent<PoseCompositor>();
        compositor.posers = new List<CharacterPoser>();


        if(mmSettings.version == MMDatabaseVersion.HOLDEN){
            var retargetingV1 = poseProvider.AddComponent<MMRuntimeRetargetingV1>();
            retargetingV1.src = controller;
            retargetingV1.retargetingMap = retargetingMapV1;
            compositor.Add(retargetingV1);
        }else{
            var retargetingV2 = poseProvider.AddComponent<MMRuntimeRetargetingV2>();
            retargetingV2.src = controller;
            retargetingV2.retargetingMap = retargetingMapV2;
            compositor.Add(retargetingV2);
        }

        var mirror= poseProvider.GetComponent<RuntimeMirroring>();
        if (mirror != null) compositor.Add(mirror);
        var ikRigBuilder = new RPMIKRigBuilder(null, false);
        CharacterRigConfig config = ikRigBuilder.BuildConfig(poseProvider);
        ac.ikControllers = new Dictionary<int, CustomTwoBoneIK>();
        var leftHandIK = poseProvider.AddComponent<CustomTwoBoneIK>();
        leftHandIK.end = config.LeftHand;
        leftHandIK.elbow = leftHandIK.end.parent;
        leftHandIK.root = leftHandIK.elbow.parent;
        ac.ikControllers[0] = leftHandIK;
        compositor.Add(leftHandIK);
        var rightHandIK = poseProvider.AddComponent<CustomTwoBoneIK>();
        rightHandIK.end = config.RightHand;
        rightHandIK.elbow = rightHandIK.end.parent;
        rightHandIK.root = rightHandIK.elbow.parent;
        ac.ikControllers[1] = rightHandIK;
        compositor.Add(rightHandIK);

        
        //compositor.Add(ac.lookat);
        ac.poseCompositor = compositor;

        ac.locomotionController = controller;
        ac.minStartDistance = minStartDistance;
        ac.minStopDistance = minStopDistance;
        ac.mirror = poseProvider.GetComponent<RuntimeMirroring>();
        ac.pdController = GetComponentInChildren<RagDollPDControllerBase>();
        ac.lookat = ac.pdController.gameObject.AddComponent<CustomLookAt>();


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