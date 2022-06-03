using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Carousel{
    
namespace BaselineAgent{

public enum AgentState{
    IDLE,
    WALK,
    DANCE,
    PAIR_DANCE
}
public class AgentController  : DanceAgentInterface
{

    NavMeshAgent navMeshAgent;
    Animator animator;
    AnimStateController stateController;
    public Transform target;
    public float minStopDistance = 0.1f;
    public float minStartDistance = 10f;//0.5f;
    public bool initiated = false;
    public float distanceToTarget;
    override public bool IsFollowing{  get { return target != null; }}  

    override public bool IsMirroring{  get { return mirror != null && mirror.active; }}  

    public AgentState state;
    public RagDollPDController controller;
    public RuntimeMirroring mirror;
    public PlayerControllerBase player;
    public PhyscisPairDanceFollower follower;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stateController = GetComponent<AnimStateController>();
        state = AgentState.IDLE;
        mirror = GetComponent<RuntimeMirroring>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistanceToTarget();
        UpdateState();
        Act();
    }
    
    public void UpdateDistanceToTarget(){
        distanceToTarget = 0;
        if (target == null) return;
        var delta = target.position - transform.position;
        distanceToTarget = delta.magnitude;
    }


    void UpdateState(){
        if(stateController.isDancing) state = AgentState.DANCE;
        if(IsPlayerDancing()) state = AgentState.DANCE;
        if(IsMirroring) state = AgentState.PAIR_DANCE;
        switch (state){
            case AgentState.IDLE:
                if ( distanceToTarget > minStartDistance && !controller.IsMirroring) state = AgentState.WALK;
                break;
            case AgentState.WALK:
                 if (distanceToTarget < minStopDistance) {
                     state = AgentState.IDLE;
                }
                break;
            case AgentState.DANCE:
                if ( distanceToTarget > minStartDistance && !controller.IsMirroring) state = AgentState.WALK;
                break;
            case AgentState.PAIR_DANCE:
                if(!IsMirroring) state = AgentState.IDLE;
                break;
        }
    }

    public void Act(){
          switch (state){
            case AgentState.IDLE:
                 if(controller!= null && !controller.active) controller.Activate();
                break;
            case AgentState.WALK:
                if(stateController.isDancing)ToggleDancing();
                navMeshAgent.SetDestination(target.position);
                if(controller!= null && controller.active ) controller.Deactivate();
                break;
            case AgentState.DANCE:
                if(!stateController.isDancing)ToggleDancing();
                break;
            case AgentState.PAIR_DANCE:
                 if(controller!= null && !controller.active) controller.Activate();
                break;
        }
        
        RotateToPlayer();
    }

    public void RotateToPlayer(){
        if (target == null || player == null) return;
        var toPlayer = player.transform.position -  transform.position;
        toPlayer.y = 0;
        toPlayer = toPlayer.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        var deltaQ = Quaternion.Inverse(transform.rotation) *targetRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, deltaQ*transform.rotation, Time.deltaTime);
    }

    public bool IsPlayerDancing(){

        if (player == null) return false;
        return player.IsDancing;
    }

/*
    public void FollowTarget(){
        if (target == null) return;
        var delta = target.position - transform.position;
        distanceToTarget = delta.magnitude;
        if ( distanceToTarget > minStartDistance){
            ToggleDancing();
            navMeshAgent.SetDestination(target.position);
         }else if(!stateController.isDancing) {
             ToggleDancing();
         }
        var deltaQ = Quaternion.Inverse(transform.rotation) *target.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, deltaQ*transform.rotation, Time.deltaTime);
    }*/

    override public void ToggleDancing(){
        stateController.ToggleDancing();
   
    }

    
    override public void Activate(PlayerInteractionZone playerInteraction)
    {   
        this.player = playerInteraction.player;
        initiated = true;
        Reset();
    }
    override public void Deactivate()
    {
      
        initiated = false;
    }

    override public void LockToLeader(Transform t){
        target = t;
    }


    override public void UnlockLeader(){

       target = null;
       Reset();
    }

   override  public void Reset(){
       
       
    }

    override public void ActivatePairDance(){
       //start mirror and stop animator
       if(mirror!=null && !mirror.active && follower!=null && player !=null){
           mirror.src = player.transform;
           mirror.active = true;
           mirror.initialized = false;
           animator.enabled = false;
           controller.IsMirroring = true;
           mirror.translationMode = RuntimeMirroring.TranslationMode.EXTERNAL;
           follower.ActivatePairDance(player);
           
       } 
       
    }
    
    override public void DeactivatePairDance(){
        //stop mirror and start animator
        
       if(mirror!=null && mirror.active && follower!=null){
           mirror.active = false;
           mirror.initialized = false;
           animator.enabled = true;
           controller.IsMirroring = false;
           follower.DeactivatePairDance();
       } 
    }

}
}
}