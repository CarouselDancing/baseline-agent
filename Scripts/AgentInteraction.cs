using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Carousel{
    
namespace BaselineAgent{
    

public class AgentInteraction : CollectableObject
{

    public DanceAgentInterface agent;

    public bool deactivateOnLeave = true; 
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

    public void ActivateAgent(PlayerInteractionZone player){
        agent?.Activate(player);
    }


    public void DeactivateAgent(){
        agent?.Deactivate();
    }

  
    public void ActivateFollower(Transform leader){
        agent?.LockToLeader(leader);

    }
    public void DeactivateFollower(){
        agent?.UnlockLeader();
    }

    public void ToggleDancing(){
        agent?.ToggleDancing();

    }
    public void ActivatePairDance(){
        agent?.ActivatePairDance();
    }

    public void DeactivatePairDance(){
        agent?.DeactivatePairDance();
    }

}
}
}