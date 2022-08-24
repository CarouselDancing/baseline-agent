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
        agent?.LockToLeader(leader);
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

    public void ActivateIK(int hand, Transform target){
        agent?.ActivateIK(hand, target);
    }

    public void DeactivateIK(int hand){
        agent?.DeactivateIK(hand);
    }

    }  
}
}