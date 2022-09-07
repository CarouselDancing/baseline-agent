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
        if(lookat == null){
            lookat = GetComponent<CustomLookAt>();
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


    override public void ToggleDancing(){
        Debug.Log("toggle dancing");
   
    }

    override public void ActivatePairDance(){
        if(mirror!=null && !mirror.active && player !=null){
           mirror.src = player.transform;
           mirror.active = true;
           mirror.initialized = false;
           //animator.enabled = false;
          // pdController.IsMirroring = true;
            mirror.translationMode = RuntimeMirroring.TranslationMode.IGNORE;
           
       } 
       
    }
    
    override public void DeactivatePairDance(){
        if(mirror!=null && mirror.active){
           mirror.active = false;
           mirror.initialized = false;
           mirror.src = null;
           pdController.Deactivate();
           pdController.delayedActivation = true;
           poseCompositor.UpdatePose();
           // animator.enabled = true;
           // pdController.IsMirroring = false;
       } 
    }

    override public void ChangeDanceStyle(){
       locomotionController.SwitchToNextDatabase();
    }

    

}
}
}