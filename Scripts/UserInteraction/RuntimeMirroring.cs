using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Carousel.FigureGenerator;

namespace Carousel{
    
namespace BaselineAgent{

[Serializable]
public class MirrorSettings
{

    public Vector3 mirrorVector;
    public Vector3 mirrorRootOffset;

    public List<FigureGeneratorSettings.RefBodyMapping> jointMap;
    public RuntimeMirroring.MirrorMode mode;
    public bool groundFeet = false;
    public string footTipName;
    public RuntimeMirroring.TranslationMode translationMode;
    public bool ignoreLowerBody = false;
};






public class RuntimeMirroring : CharacterPoser
{
    public enum MirrorMode
    {
        GLOBAL = 1,
        LOCAL = 2
    }

    public enum TranslationMode
    {
        ABSOLUTE = 1,
        RELATIVE = 2,
        EXTERNAL = 3,
        IGNORE = 4
    }
    [Serializable]
    public class JointMap
    {
        public string src;
        public string dst;
        public Transform srcT;
        public Transform dstT;

    }
    public Transform src;
    private List<Transform> _transforms;
    private List<Transform> _srcTransforms;
    public List<JointMap> jointMap;

    JointMap rootMap;
    public Vector3 mirrorVector;
    public Matrix4x4 mirrorMatrix;
    public Vector3 relativeRootOffset;
    public MirrorMode mode = MirrorMode.GLOBAL;
    public TranslationMode translationMode = TranslationMode.RELATIVE;
    public bool applyRootPosition = true;
    public Vector3 lastPosition;
    public bool rootPosSet = false;
    public bool groundFeet = false;
    public Transform footTip;

    public float footTipOffset =0;
    public Vector3 externalPosition;
    public string rootName;

    
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!active || inPipeline)return;
        //transform.position = src.GetGlobalPosition(Bones.Entity);
        if (!initialized) SetTransforms();
        UpdatePose();
    }

    override public void UpdatePose(){
        if(!initialized)SetTransforms();
        MirrorPose();
    }


    override public void SetTransforms()
    {
        if(src==null)return;
        mirrorMatrix.m00 = mirrorVector.x;
        mirrorMatrix.m11 = mirrorVector.y;
        mirrorMatrix.m22 = mirrorVector.z;
        mirrorMatrix.m33 = 1;
        _transforms = GetComponentsInChildren<Transform>().ToList();
        _srcTransforms = src.GetComponentsInChildren<Transform>().ToList();

        for (int i = 0; i < jointMap.Count; i++)
        {
            if (jointMap[i].src == "ignore") continue;

            var srcTs = _srcTransforms.FindAll(x => x.name == jointMap[i].dst);
            if(srcTs.Count > 0){
                jointMap[i].srcT =srcTs[0];
            }else{
                Debug.Log("error: could not find"+ jointMap[i].dst);
            }
            Transform dstT;

            dstT = _transforms.First(x => x.name == jointMap[i].src);
            /*
            if (mode == MirrorMode.GLOBAL) { 
            }
            else { 
                dstT = _transforms.First(x => x.name == jointMap[i].dst);
            }*/
            jointMap[i].dstT = dstT;
            if (jointMap[i].src == rootName)
            {
                rootMap = jointMap[i];
            }
            
        }
        initialized = true;

    }

    public void MirrorPose()
    { 
        if (mode == MirrorMode.GLOBAL)
        {
            if(translationMode == TranslationMode.ABSOLUTE){
                MirrorPoseGlobal();
           }else if(translationMode == TranslationMode.EXTERNAL) {
                MirrorPoseGlobalWithExternalTranslation();
            }else{
                MirrorPoseGlobalWithRelativeTranslation();
            }
        }
        else
        {
           MirrorPoseLocal();
        }
    }
    public void MirrorPoseGlobal()
    {
        var srcRootRotation = rootMap.srcT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);

        Vector3 _relativeRootOffset = srcRootRotation * relativeRootOffset;
        //_relativeRootOffset.y = 0;
        MirrorJointRotationsAndPositions(srcRootRotation, _relativeRootOffset, Vector3.zero, true); 
        ApplyFootGrounding();
    }
    
    public void MirrorPoseGlobalWithExternalTranslation()
    {
        var srcRootRotation = rootMap.srcT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);
        Vector3 _relativeRootOffset = MirrorRootTransform(srcRootRotation);
        var absoluteMirroredRootPos = _relativeRootOffset + rootMap.srcT.position;
        Vector3 deltaToRootPos = externalPosition-absoluteMirroredRootPos;
        deltaToRootPos.y = 0;
        MirrorJointRotationsAndPositions(srcRootRotation, _relativeRootOffset, deltaToRootPos); 
        ApplyFootGroundingV2();
    }
    
    public void MirrorPoseGlobalWithRelativeTranslation()
    {
        var srcRootRotation = rootMap.srcT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);
        Vector3 _relativeRootOffset = MirrorRootTransform(srcRootRotation);
        //_relativeRootOffset.y = 0;
        var absoluteMirroredRootPos = _relativeRootOffset + rootMap.srcT.position;
        Vector3 deltaToRootPos = rootMap.dstT.position-absoluteMirroredRootPos;
        
        MirrorJointRotationsAndPositions(srcRootRotation, _relativeRootOffset, deltaToRootPos); 
        ApplyFootGrounding();

    }

    public void ApplyFootGrounding(){
        if (groundFeet && footTip != null && footTip.position.y != 0)
        {

            rootMap.dstT.position += new Vector3(0,-footTip.position.y+footTipOffset,0);
        }
    }
    public void ApplyFootGroundingV2(){
        if (!groundFeet || footTip != null) return;
        float deltaToZero = footTip.position.y+footTipOffset;
        if (deltaToZero == 0) return;
        foreach (var m in jointMap)
        {
            if (m.src == "ignore" ) continue;
                m.dstT.position += new Vector3(0,-deltaToZero,0);
        }
    }


    public void MirrorPoseLocal()
    {
        var srcRootRotation = rootMap.srcT.rotation;
        srcRootRotation = Quaternion.Euler(0, srcRootRotation.eulerAngles.y, 0);
        if(translationMode != TranslationMode.IGNORE) MirrorRootTransform(srcRootRotation);
        ApplyFootGrounding();
        MirrorJointRotations(srcRootRotation);
    }


    public Vector3 MirrorRootTransform(Quaternion srcRootRotation){
        var dstRootRotation = rootMap.dstT.rotation;
        var invRootRotation = Quaternion.Inverse(srcRootRotation);
        var _relativeRootOffset = srcRootRotation * relativeRootOffset;
        Matrix4x4 matrix = mirrorMatrix * Matrix4x4.TRS(Vector3.zero, invRootRotation * srcRootRotation, Vector3.one) * mirrorMatrix;
        rootMap.dstT.rotation = srcRootRotation * matrix.rotation;
        rootMap.dstT.localRotation *= Quaternion.Euler(0, 180, 0);
        if (!applyRootPosition) return _relativeRootOffset;
        var absoluteMirroredRootPos = _relativeRootOffset + rootMap.srcT.position;
     
        switch (translationMode)
        {
            case TranslationMode.ABSOLUTE:
                rootMap.dstT.position = absoluteMirroredRootPos;
                break;
            case TranslationMode.RELATIVE:
                if (!rootPosSet)
                {
                    rootMap.dstT.position = absoluteMirroredRootPos;
                }
                else
                {
                    var delta = rootMap.srcT.position - lastPosition;
                    matrix = mirrorMatrix * Matrix4x4.TRS(delta, Quaternion.identity, Vector3.one) * mirrorMatrix;
                    delta = matrix.GetColumn(3);
                    delta.y = 0; //HACK TO prevent moving up and down
                    rootMap.dstT.position = rootMap.dstT.position + delta;
                }
                lastPosition = rootMap.srcT.position;
                rootPosSet = true;
                break;
        };
        return _relativeRootOffset;      
    }

    public void MirrorJointRotations(Quaternion srcRootRotation)
    {
        var invRootRotation = Quaternion.Inverse(srcRootRotation);
        Matrix4x4 matrix;
        foreach (var m in jointMap){
            if (m.dst == rootName) continue;
            var srcT = m.srcT;
            var dstT = m.dstT;
            matrix = mirrorMatrix * Matrix4x4.TRS(Vector3.zero, invRootRotation * srcT.rotation, Vector3.one) * mirrorMatrix;
            dstT.rotation = srcRootRotation * matrix.rotation;
            dstT.localRotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    public void MirrorJointRotationsAndPositions(Quaternion srcRootRotation, Vector3 relativeRootOffset, Vector3 deltaToRootPos, bool includeRoot = false){
        var invRootRotation = Quaternion.Inverse(srcRootRotation);
        foreach (var m in jointMap)
        {
            if (m.src == "ignore" || (!includeRoot || m.src == rootName)) continue;

            var srcT = m.srcT;
            var dstT = m.dstT;
            var relativePos = invRootRotation * (srcT.position - rootMap.srcT.position);
            var matrix = mirrorMatrix * Matrix4x4.TRS(relativePos, invRootRotation * srcT.rotation, Vector3.one) * mirrorMatrix;

           
            dstT.rotation = srcRootRotation * matrix.rotation;
            dstT.localRotation *= Quaternion.Euler(0, 180, 0);

            Vector3 delta = matrix.GetColumn(3);
            dstT.position = relativeRootOffset + rootMap.srcT.position + srcRootRotation * delta;
            dstT.position+= deltaToRootPos;
        }
    }

    public void SnapTo(Vector3 snapPosition)
    {
        gameObject.SetActive(false);
        transform.position = snapPosition;
        gameObject.SetActive(true);
    }

    public void SetExternalTargetPosition(Vector3 t){
        externalPosition = t;
    }


    public void ResetToIdle(){

            rootPosSet = false;
            MirrorPose();
    }

    public static List<RuntimeMirroring.JointMap> CreateHumanoidMirrorMap(GameObject model)
    {
        var anim = model.GetComponent<Animator>();
        var jointMap = new List<RuntimeMirroring.JointMap>();
        var leftLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).name;
        var rightLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftLeg, dst = rightLeg });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightLeg, dst = leftLeg });
        var leftLowerLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).name;
        var rightLowerLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftLowerLeg, dst = rightLowerLeg });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightLowerLeg, dst = leftLowerLeg });
        var leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot).name;
        var rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftFoot, dst = rightFoot });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightFoot, dst = leftFoot });


        var hips = anim.GetBoneTransform(HumanBodyBones.Hips).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = hips, dst = hips });
        var spine = anim.GetBoneTransform(HumanBodyBones.Spine).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = spine, dst = spine });
        var chest = anim.GetBoneTransform(HumanBodyBones.Chest).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = chest, dst = chest });
        var upperChest = anim.GetBoneTransform(HumanBodyBones.Chest).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = upperChest, dst = upperChest });
        var head = anim.GetBoneTransform(HumanBodyBones.Head).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = head, dst = head });
        var leftArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm).name;
        var rightArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftArm, dst = rightArm });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightArm, dst = leftArm });
        var leftLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm).name;
        var rightLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftLowerArm, dst = rightLowerArm });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightLowerArm, dst = leftLowerArm });

        return jointMap;
    }

    public static List<RuntimeMirroring.JointMap> CreateUpperBodyHumanoidMirrorMap(GameObject model)
    {
        var anim = model.GetComponent<Animator>();
        var jointMap = new List<RuntimeMirroring.JointMap>();
        var hips = anim.GetBoneTransform(HumanBodyBones.Hips).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = hips, dst = hips });
        var spine = anim.GetBoneTransform(HumanBodyBones.Spine).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = spine, dst = spine });
        var chest = anim.GetBoneTransform(HumanBodyBones.Chest).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = chest, dst = chest });
        var upperChest = anim.GetBoneTransform(HumanBodyBones.Chest).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = upperChest, dst = upperChest });
        var head = anim.GetBoneTransform(HumanBodyBones.Head).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = head, dst = head });
        var leftArm = anim.GetBoneTransform(HumanBodyBones.RightUpperArm).name;
        var rightArm = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftArm, dst = rightArm });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightArm, dst = leftArm });
        var leftLowerArm = anim.GetBoneTransform(HumanBodyBones.RightLowerArm).name;
        var rightLowerArm = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).name;
        jointMap.Add(new RuntimeMirroring.JointMap { src = leftLowerArm, dst = rightLowerArm });
        jointMap.Add(new RuntimeMirroring.JointMap { src = rightLowerArm, dst = leftLowerArm });

        return jointMap;
    }

}

}
}