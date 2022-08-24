using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

namespace Carousel{
    
namespace BaselineAgent{

public class AnimatorNetworkAgentController  : NetworkAgentController
{

    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public AnimStateController stateController;


    void Start()
    {
        if(navMeshAgent == null){
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            stateController = GetComponent<AnimStateController>();
            mirror = GetComponent<RuntimeMirroring>();
        }
        state = AgentState.IDLE;
    }

    // Update is called once per frame
    void Update()
    {
         if(!isServer || stateController == null )return;
        UpdateDistanceToTarget();
        UpdateState();
        Act();
    }
 


    void UpdateState(){
        if(stateController.isDancing) state = AgentState.DANCE;
        if(IsPlayerDancing()) state = AgentState.DANCE;
        if(IsMirroring) state = AgentState.PAIR_DANCE;
        switch (state){
            case AgentState.IDLE:
                if ( distanceToTarget > minStartDistance && !pdController.IsMirroring) state = AgentState.WALK;
                break;
            case AgentState.WALK:
                 if (distanceToTarget < minStopDistance) {
                     state = AgentState.IDLE;
                }
                break;
            case AgentState.DANCE:
                if ( distanceToTarget > minStartDistance && !pdController.IsMirroring) state = AgentState.WALK;
                break;
            case AgentState.PAIR_DANCE:
                if(!IsMirroring) state = AgentState.IDLE;
                break;
        }
    }

    public void Act(){
          switch (state){
            case AgentState.IDLE:
                 if(pdController!= null && !pdController.IsActive) pdController.Activate();
                break;
            case AgentState.WALK:
                if(stateController.isDancing)ToggleDancing();
                navMeshAgent.SetDestination(target.position);
                if(pdController!= null && pdController.IsActive ) pdController.Deactivate();
                break;
            case AgentState.DANCE:
                if(!stateController.isDancing)ToggleDancing();
                break;
            case AgentState.PAIR_DANCE:
                 if(pdController!= null && !pdController.IsActive) pdController.Activate();
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

    bool IsPlayerDancing(){

        if (player == null) return false;
        return player.IsDancing;
    }

    override public void ToggleDancing(){
        Debug.Log("toggle dancing");
        stateController.ToggleDancing();
   
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
    }  

    public void LockToLeader(Transform t){
        target = t;
    }


    public void UnlockLeader(){

       target = null;
       Reset();
    }

    public void Reset(){
       
       
    }

    public void SetHighlightMode(bool active){
        highlight?.SetMode(active);
    }


    override public void ActivatePairDance(){
       //start mirror and stop animator
       if(mirror!=null && !mirror.active && follower!=null && player !=null){
           mirror.src = player.transform;
           mirror.active = true;
           mirror.initialized = false;
           animator.enabled = false;
           pdController.IsMirroring = true;
           mirror.translationMode = RuntimeMirroring.TranslationMode.EXTERNAL;
            Debug.Log("ActivatePairDance");
           follower.ActivatePairDance(player);
           
       } 
       
    }
    
    override public void DeactivatePairDance(){
        //stop mirror and start animator
       if(mirror!=null && mirror.active && follower!=null){
           mirror.active = false;
           mirror.initialized = false;
           animator.enabled = true;
           pdController.IsMirroring = false;
            Debug.Log("DeactivatePairDance");
           follower.DeactivatePairDance();
       } 
    }

}
}
}