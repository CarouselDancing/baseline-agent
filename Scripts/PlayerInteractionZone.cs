using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Carousel.BaselineAgent;

namespace Carousel
{
    

public class PlayerInteractionZone : ObjectCollectionZone
{ 
    public bool IsLeading;
    public bool IsPairDancing;
    public AgentInteraction agent;
    public PlayerControllerBase player;
    public Transform partnerTarget;

    // public List<ABGrabber> grabbers;
    /*public void SetCurrentAgentZone(AgentInteractionZone az){
        if(agentZone != az)DeactivateAgent(agentZone);
        agentZone = az;
    }*/
    public void RemoveAgentZone(){
        this.agent = null;
    }

    // called by button press
    public void ActivateAgent(){
        
        agent = (AgentInteraction) collection.Where(c => c is AgentInteraction).FirstOrDefault();
        ResetGrabbers();
       if(agent != null) agent.ActivateAgent(this);
    }

    public void DeactivateAgent(){
        ResetGrabbers();
        if(agent != null) {
            agent.DeactivateAgent();
            DeactivateFollower();
        }
        RemoveAgentZone();
    }

    //called when exiting the zone
    public void DeactivateAgent(AgentInteraction az){
        ResetGrabbers();
        if(agent == az && az != null) agent.DeactivateAgent();
        RemoveAgentZone();
    }

    public void ResetGrabbers(){
       //  foreach(var g in grabbers){
      //       g.RemoveGrabbableObject();
      //   }
    }

    public void ActivateFollower(){
        if(agent != null && !IsLeading) {
            agent.ActivateFollower(partnerTarget);
            IsLeading = true;
        }
    }

       public void DeactivateFollower(){
        if(agent != null) {
            agent.DeactivateFollower();
            IsLeading = false;
        }
    }

    public void ToggleDancing(){
        if(agent != null) {
            agent.ToggleDancing();
        }

    }

    public void ActivatePairDance(){
        if(agent != null) {
            agent.ActivatePairDance();
            IsPairDancing = true;
        }
    }

    public void DeactivatePairDance(){
        if(agent != null) {
            agent.DeactivatePairDance();
            IsPairDancing = false;
        }
    }



    /*
     void  OnTriggerEnter (Collider other){
        var zone = other.GetComponent<AgentInteractionZone>();
        if (agentZone!=null && IsLeading)return;
        SetCurrentAgentZone(zone);
    }

    void  OnTriggerExit (Collider other){
        var zone = other.GetComponent<AgentInteractionZone>();
        if (agentZone!=null && IsLeading)return;
        if (agentZone != null && agentZone.deactivateOnLeave) {
            DeactivateAgent(zone);
        }else{
            RemoveAgentZone();
        }
    }*/

}

}