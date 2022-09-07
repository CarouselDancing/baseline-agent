/* base class for network agent */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

namespace Carousel{
    
namespace BaselineAgent{

public class NetworkAgentController  : NetworkBehaviour
{

    public Transform target;
    public float minStopDistance = 0.1f;
    public float minStartDistance = 10f;//0.5f;
    public bool initiated = false;
    public float distanceToTarget;
    public bool IsFollowing{  get { return target != null; }}  

    public bool IsMirroring{  get { return mirror != null && mirror.active; }}  

    public AgentState state  = AgentState.IDLE;
    public RagDollPDControllerBase pdController;
    public RuntimeMirroring mirror;
    public CustomLookAt lookat;
    public PlayerControllerBase player;
    public PhysicsPairDanceFollower follower;
    public HighlightGroupController highlight;
    public PoseCompositor poseCompositor;
    public Dictionary<int, CustomTwoBoneIK> ikControllers;
    
    public bool useMirrorOnDefault = false;
   
    public void UpdateDistanceToTarget(){
        distanceToTarget = 0;
        if (target == null) return;
        var delta = target.position - transform.position;
        distanceToTarget = delta.magnitude;
    }
 
    bool IsPlayerDancing(){

        if (player == null) return false;
        return player.IsDancing;
    }

    virtual public void ToggleDancing(){
        Debug.Log("toggle dancing");
   
    }

    
    public void Activate(PlayerInteractionZone playerInteraction)
    {   
        this.player = playerInteraction.player;
        initiated = true;
        Reset();
    }

        

    public void Deactivate()
    {
        initiated = false;
        lookat.targetTransform = null;
    }  

    public void LockToLeader(Transform t){
        
        Debug.Log("lock to leader", this.player.GetComponent<Animator>());
        lookat.targetTransform = Camera.main.transform;//this.player.GetComponent<Animator>().GetBoneTransform(lookat.targetBone);
        target = t;
    }


    public void UnlockLeader(){

       target = null;
       lookat.targetTransform = null;
       Reset();
    }

    public void Reset(){
    }

    public void SetHighlightMode(bool active){
        highlight?.SetMode(active);
    }


    virtual public void ActivatePairDance(){
      
       
    }
    
    virtual public void DeactivatePairDance(){
       
    }


    public Transform ActivateIK(int hand, Transform target){
        ikControllers[hand].target = target;
        var poseProviderJoint = ikControllers[hand].end;
        return poseProviderJoint;
    }

    
    public void DeactivateIK(int hand){
        ikControllers[hand].target = null;
    }

    virtual public void ChangeDanceStyle(){
        
    }
}
}
}