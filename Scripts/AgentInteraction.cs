using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Carousel{
    
namespace BaselineAgent{
    

public class AgentInteraction : CollectableObject
{

    public DanceAgentInterface agent;

    public bool deactivateOnLeave = false; 
/*
    void  OnTriggerEnter (Collider other){
        if (agent == null) return;
        var t = other.GetComponent<AgentInteraction>();
        if (t==null || t.IsLeading)return;
        t.SetCurrentAgentZone(this);
    }

    void  OnTriggerExit (Collider other){
        if (agent == null) return;
        var t = other.GetComponent<AgentInteraction>();
        if (t==null || t.IsLeading)return;
        if (deactivateOnLeave) {
            t.DeactivateAgent(this);
        }else{
            t.RemoveAgentZone();
        }
    }
    */

    virtual public void ActivateAgent(PlayerInteractionZone player){
        agent?.Activate(player);
    }


    virtual public void DeactivateAgent(){
        agent?.Deactivate();
    }

  
    virtual public void ActivateFollower(Transform leader){
        agent?.LockToLeader(leader);

    }
    virtual public void DeactivateFollower(){
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

}
}
}