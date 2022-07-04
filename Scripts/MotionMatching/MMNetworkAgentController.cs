using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Carousel.MotionMatching;

namespace Carousel{
    
namespace BaselineAgent{

public class MMNetworkAgentController : NetworkAgentController
{

    public TargetLocomotionController locomotionController;
    void Start()
    {
        if(mirror == null){
            mirror = GetComponent<RuntimeMirroring>();
        }
        
        state = AgentState.IDLE;
    }

    void Update()
    {
         if(!isServer ||locomotionController==null)return;
        UpdateDistanceToTarget();
        UpdateState();
        Act();

    }
    

    void UpdateState(){
        switch (state){
            case AgentState.IDLE:
                if ( distanceToTarget > minStartDistance) state = AgentState.WALK;
                break;
            case AgentState.WALK:
                 if (distanceToTarget < minStopDistance) {
                     state = AgentState.IDLE;
                }
                break;
        }
    }

    public void Act(){
          switch (state){
            case AgentState.IDLE:
                locomotionController.target = null;
                break;
            case AgentState.WALK:
                locomotionController.target = target;
                break;
        }
    }


    bool IsPlayerDancing(){

        if (player == null) return false;
        return player.IsDancing;
    }

    override public void ToggleDancing(){
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
       
       
    }
    
    override public void DeactivatePairDance(){
     
    }

}
}
}