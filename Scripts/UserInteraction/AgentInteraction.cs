using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Carousel{
    
namespace BaselineAgent{
    

public class AgentInteraction : CollectableObject
{

    public NetworkAgentController agent;

    public bool deactivateOnLeave = false; 


    virtual public void ActivateAgent(PlayerInteractionZone player){
        agent?.Activate(player);
    }


    virtual public void DeactivateAgent(){
        agent?.Deactivate();
    }

  
    virtual public void ActivateFollower(Transform leader){
        if(agent== null)return;
        agent.LockToLeader(leader);
        if(agent.useMirrorOnDefault)
        agent?.ActivatePairDance();

    }
    virtual public void DeactivateFollower(){
        agent?.DeactivatePairDance();
        agent?.UnlockLeader();
    }

    virtual public void ToggleDancing(){
        agent?.ToggleDancing();

    }
    virtual public void ActivatePairDance(){
        agent?.ActivatePairDance();
    }

    virtual public void DeactivatePairDance(){
        agent?.DeactivatePairDance();
    }

    public void SetHighlightMode(bool active){
        agent?.SetHighlightMode(active);
    }

    public Transform ActivateIK(int hand, Transform target){
        Transform t = null;
        if(agent != null) t = agent.ActivateIK(hand, target);
        return t;
    }

    public void DeactivateIK(int hand){
        agent?.DeactivateIK(hand);
    }
    public void ChangeDanceStyle(){
        agent?.ChangeDanceStyle();
    }

    }  
}
}