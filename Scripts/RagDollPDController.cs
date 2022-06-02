using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace Carousel{
    
namespace BaselineAgent{


public class RagDollPDController : RagDollPDControllerBase
{

    [Serializable]
    public class BodyMap{
        public Transform src;
        public Transform dst;
    }


    List<Rigidbody> _mocapBodyParts;
    List<ArticulationBody> _mocapBodyPartsA;
    List<ArticulationBody> _bodyParts;
    RagDoll003 _ragDollSettings;
    List<ArticulationBody> _bodies;
    List<ArticulationBody> _motors;

    public int numActionDims;

    public bool _hasLazyInitialized;
    public float[] _mocapTargets;
    public bool activateExternalForce;
    public  ArticulationBody root;
    public float kp = 500f;//0.001f; // proportional gain
    public float rkp = 5;//0.001f; // proportional gain
    public float kd = 10f; // differential gain
    public float limit = 10e14f;

    public Vector3 rootForce;
    public Vector3 rootTorque;
    public Transform targetRoot;
    public List<BodyMap> bodyMap;

    public int solverIterations = 255;
    
    public bool IsMirroring;

     
    void FixedUpdate()
    {
        if (!_hasLazyInitialized)
        {
            OnEpisodeBegin();
            return;
        }
        if(!active){
             foreach (var m in bodyMap){
                m.dst.transform.position = m.src.position;
                m.dst.transform.rotation = m.src.rotation;
             }
            return;
        };
        var vectorAction = GetMocapTargets();
    
        int i = 0;
        foreach (var m in _motors)
        {
            if (m.isRoot)
                continue;
            Vector3 targetNormalizedRotation = Vector3.zero;

            if (m.twistLock == ArticulationDofLock.LimitedMotion)
                targetNormalizedRotation.x = vectorAction[i++];
            if (m.swingYLock == ArticulationDofLock.LimitedMotion)
                targetNormalizedRotation.y = vectorAction[i++];
            if (m.swingZLock == ArticulationDofLock.LimitedMotion)
                targetNormalizedRotation.z = vectorAction[i++];
            UpdateMotor(m, targetNormalizedRotation);
        }
        if(activateExternalForce)ApplyExternalForceOnRoot();
        if(!IsMirroring && animationSrc!= null){
            var stateController = animationSrc.GetComponent<AnimStateController>();
            if(stateController!= null)stateController.applyRootMotion = false;
            animationSrc.GetComponent<Animator>().applyRootMotion = false;
            SetReferenceRootTransform();
        }
       
    }

    public void ApplyExternalForceOnRoot(){
        if(root == null) return;
         var delta = (targetRoot.position - root.transform.position)*Time.fixedDeltaTime;
         //delta.y = 0;
         rootForce = kp * delta;
         transform.position += delta;
        var deltaV = (targetRoot.GetComponent<Rigidbody>().velocity - root.velocity);
        rootForce += kd * deltaV;
        root.AddForce(root.mass * rootForce);
        rootForce = Vector3.ClampMagnitude(rootForce, limit);
        var deltaQ = Quaternion.Inverse(root.transform.rotation)*targetRoot.rotation;
        var deltaY = deltaQ.eulerAngles.y*Mathf.Deg2Rad;
        rootTorque = rkp * Vector3.up*deltaY*Time.fixedDeltaTime/2;
        //float angle;
        //deltaQ.ToAngleAxis( out angle, out rootTorque);
        rootTorque = Vector3.ClampMagnitude(rootTorque, limit);
        root.AddTorque(root.mass * rootTorque);
        foreach (var m in _motors)
        {
            m.AddForce(m.mass*rootForce);
            m.AddTorque(m.mass*rootTorque);
        }
        //animationSrc.stabilizer.GetComponent<Rigidbody>().AddForce(rootForce);

    }


   public override void OnEpisodeBegin()
    {

        if (!_hasLazyInitialized && animationSrc != null)
        {
         
            _mocapBodyParts = animationSrc.GetComponentsInChildren<Rigidbody>().ToList();
            _mocapBodyPartsA = animationSrc.GetComponentsInChildren<ArticulationBody>().ToList();
    
            _bodyParts = GetComponentsInChildren<ArticulationBody>().ToList();
            _ragDollSettings = GetComponent<RagDoll003>();

            root = GetComponentsInChildren<ArticulationBody>().Where(x => x.isRoot).FirstOrDefault();

             if (_mocapBodyParts.Count > 0) {
                Rigidbody mocapBody = _mocapBodyParts.First(x => x.name == animationSrc.rootName);
                targetRoot = mocapBody.transform;
            }
            else
            {
                ArticulationBody mocapBody = _mocapBodyPartsA.First(x => x.name ==animationSrc.rootName);
                targetRoot = mocapBody.transform;
            }
            _bodies =  GetComponentsInChildren<ArticulationBody>().ToList();

            bodyMap = new List<BodyMap>();
            bodyMap.Add(new BodyMap(){dst= root.transform, src = targetRoot.transform});
            foreach (var b in _bodies){
                var src = animationSrc.GetComponentsInChildren<Transform>().First(x => x.name == b.name);
                bodyMap.Add(new BodyMap(){dst= b.transform, src = src});
            }
            _motors = _bodies
                .Where(x => x.jointType == ArticulationJointType.SphericalJoint)
                .Where(x => !x.isRoot)
                .Distinct()
                .ToList();
            var individualMotors = new List<float>();
            numActionDims = 0;
           
            _mocapTargets = null;
            _hasLazyInitialized = true;
            animationSrc.ResetToIdle();
            animationSrc.CopyStatesTo(this.gameObject, false);
        }
        OnReset?.Invoke();

#if UNITY_EDITOR
        if (DebugPauseOnReset)
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
#endif	      
    }

    float[] GetMocapTargets()
    {
        if (_mocapTargets == null)
        {
            _mocapTargets = _motors
                .Where(x => !x.isRoot)
                .SelectMany(x => {
                    List<float> list = new List<float>();
                    if (x.twistLock == ArticulationDofLock.LimitedMotion)
                        list.Add(0f);
                    if (x.swingYLock == ArticulationDofLock.LimitedMotion)
                        list.Add(0f);
                    if (x.swingZLock == ArticulationDofLock.LimitedMotion)
                        list.Add(0f);
                    return list.ToArray();
                })
                .ToArray();
        }
        int i = 0;
        foreach (var joint in _motors)
        {
            if (joint.isRoot)
                continue;
            Quaternion localRot;
            Transform mocapBodyTransform;
             Vector3 targetRotationInJointSpace;
            if (_mocapBodyParts.Count > 0) {
                Rigidbody mocapBody = _mocapBodyParts.First(x => x.name == joint.name);
                mocapBodyTransform = mocapBody.transform;
                localRot = mocapBodyTransform.localRotation;
                if (mocapBodyTransform.parent.GetComponent<Rigidbody>() == null)
                {
                    localRot = mocapBodyTransform.parent.localRotation * localRot;
                }
            }
            else
            {
                ArticulationBody mocapBody = _mocapBodyPartsA.First(x => x.name == joint.name);
                mocapBodyTransform = mocapBody.transform;
                localRot = mocapBodyTransform.localRotation;
            }
           
            targetRotationInJointSpace = -(Quaternion.Inverse(joint.anchorRotation) * Quaternion.Inverse(localRot) * joint.parentAnchorRotation).eulerAngles;
            targetRotationInJointSpace = new Vector3(
                Mathf.DeltaAngle(0, targetRotationInJointSpace.x),
                Mathf.DeltaAngle(0, targetRotationInJointSpace.y),
                Mathf.DeltaAngle(0, targetRotationInJointSpace.z));
            if (joint.twistLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = joint.xDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var target = (targetRotationInJointSpace.x - midpoint) / scale;
                _mocapTargets[i] = target;
                i++;
            }
            if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = joint.yDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var target = (targetRotationInJointSpace.y - midpoint) / scale;
                _mocapTargets[i] = target;
                i++;
            }
            if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
            {
                var drive = joint.zDrive;
                var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
                var midpoint = drive.lowerLimit + scale;
                var target = (targetRotationInJointSpace.z - midpoint) / scale;
                _mocapTargets[i] = target;
                i++;
            }
        }
        return _mocapTargets;
    }  
    
    void UpdateMotor(ArticulationBody joint, Vector3 targetNormalizedRotation)
    {
        Vector3 power = _ragDollSettings.MusclePowers.First(x => x.Muscle == joint.name).PowerVector;
        power *= _ragDollSettings.Stiffness;
        float damping = _ragDollSettings.Damping;

        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.xDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.x * scale);
            drive.target = target;
            drive.stiffness = power.x;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.yDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.y * scale);
            drive.target = target;
            drive.stiffness = power.y;
            drive.damping = damping;
            joint.yDrive = drive;
        }

        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.zDrive;
            var scale = (drive.upperLimit - drive.lowerLimit) / 2f;
            var midpoint = drive.lowerLimit + scale;
            var target = midpoint + (targetNormalizedRotation.z * scale);
            drive.target = target;
            drive.stiffness = power.z;
            drive.damping = damping;
            joint.zDrive = drive;
        }
    }

    void DeactivateMotor(ArticulationBody joint)
    {
        Vector3 power = _ragDollSettings.MusclePowers.First(x => x.Muscle == joint.name).PowerVector;
        power *= _ragDollSettings.Stiffness;
        float damping = _ragDollSettings.Damping;

        if (joint.twistLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.xDrive;
            drive.stiffness =0;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        if (joint.swingYLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.yDrive;
            drive.stiffness = 0;
            drive.damping = damping;
            joint.yDrive = drive;
        }

        if (joint.swingZLock == ArticulationDofLock.LimitedMotion)
        {
            var drive = joint.zDrive;
            drive.stiffness = 0;
            drive.damping = damping;
            joint.zDrive = drive;
        }
    }

    void DeactivateBody(ArticulationBody body)
    {
       body.enabled = false;
         //joint.Sleep();
    }

    void ActivateBody(ArticulationBody body)
    {
        body.enabled = true;
        
        body.solverIterations = solverIterations;
        body.solverVelocityIterations = solverIterations;
        //joint.WakeUp();
    }

    public void Deactivate(){
        active = false;
        foreach (var m in _bodies)
        {
          DeactivateBody(m);
        }
        if(!IsMirroring) SetReferenceRootTransform();
    }

     public void Activate(){
        active = true;
        foreach (var m in _bodies)
        {
            ActivateBody(m);
        }
         if(!IsMirroring)animationSrc.CopyStatesTo(this.gameObject, false);
    }

    void SetReferenceRootTransform(){
        var delta =root.transform.position - targetRoot.position;
        delta.y = 0;
        var deltaQ = Quaternion.Inverse(targetRoot.rotation)*root.transform.rotation;
        var deltaY = deltaQ.eulerAngles.y;
        deltaQ = Quaternion.AngleAxis(deltaY, Vector3.up);
        animationSrc.transform.position += delta;
        animationSrc.transform.rotation *= deltaQ;
        
    }


}

}
}