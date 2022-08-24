using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Carousel{
    
namespace BaselineAgent{
    

public class PhysicsPairDanceFollower : PairDanceFollower
{

    public enum FollowMode{
        TRANSFORM = 0, 
        FORCES = 1,
        JOINT = 2
    }
    public Transform reference;
    public RuntimeMirroring mirror;
    public ArticulationBody root;
    public Vector3 targetPosition;
    public float kp;
    public FollowMode mode;
    public Rigidbody partnerRoot; 
    public ConfigurableJoint partnerRootJoint;

    override public void ActivatePairDance(PlayerControllerBase player){
        partnerRootJoint = null;
        mirror.translationMode = RuntimeMirroring.TranslationMode.EXTERNAL;
        leader = player.transform;
        partnerRoot = player.root.GetComponent<Rigidbody>();

        switch (mode){
            case FollowMode.TRANSFORM:
                root.immovable = true;
                break;
            case FollowMode.FORCES:
                root.useGravity = false;
                break;
            case FollowMode.JOINT:
                CreateJoint();
               
                break;

        }
        offset = root.transform.position - leader.position;
        offset.y = 0;
        offset = Quaternion.Inverse(leader.rotation)* offset;
        offsetRotation = Quaternion.Inverse(leader.rotation)*  root.transform.rotation;
    }

    public void CreateJoint(){
        partnerRootJoint = partnerRoot.gameObject.AddComponent<ConfigurableJoint>();
        partnerRootJoint.angularXMotion = ConfigurableJointMotion.Locked;
        partnerRootJoint.angularYMotion = ConfigurableJointMotion.Locked;
        partnerRootJoint.angularZMotion = ConfigurableJointMotion.Locked;
        partnerRootJoint.xMotion = ConfigurableJointMotion.Locked;
        partnerRootJoint.yMotion = ConfigurableJointMotion.Locked;
        partnerRootJoint.zMotion = ConfigurableJointMotion.Locked;
        partnerRootJoint.connectedArticulationBody = root;

    }

    public void RemoveJoint(){
        if(partnerRootJoint != null)DestroyImmediate(partnerRootJoint);
        partnerRootJoint = null;

    }

    override public void DeactivatePairDance()
    {
        leader = null;
          switch (mode){
            case FollowMode.TRANSFORM:
                root.immovable = false;
                break;
            case FollowMode.FORCES:
                root.useGravity = true;
                break;
            case FollowMode.JOINT:
                RemoveJoint();                
                break;

        }
        mirror.translationMode = RuntimeMirroring.TranslationMode.RELATIVE;

    }
    void FixedUpdate()
    {
        if (leader != null && root != null && mirror!= null){

            var o = leader.rotation* offset;
            targetPosition = leader.position+ o;
            var delta = targetPosition - root.transform.position;
            delta.y = 0;
           switch (mode){
            case FollowMode.TRANSFORM:
                root.TeleportRoot(root.transform.position + delta,  root.transform.rotation );
                break;
            case FollowMode.FORCES:
                root.AddForce(delta*Time.fixedDeltaTime*kp);
                var newRotation = offsetRotation*leader.rotation;
                 var deltaRotation = Quaternion.Inverse(root.transform.rotation)*  root.transform.rotation;
                float angle;
                Vector3 axis;
                deltaRotation.ToAngleAxis(out angle, out axis);
                root.AddTorque(axis*angle /2 * Time.fixedDeltaTime*kp);
                break;
            }
            if(reference != null){
                delta = targetPosition - reference.position;
                delta.y = 0;
                reference.position += delta;
                reference.rotation =  offsetRotation*leader.rotation;
            }
            mirror.SetExternalTargetPosition(targetPosition);
        }
    }

    
}
}
}