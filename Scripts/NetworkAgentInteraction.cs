using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Carousel{
    
namespace BaselineAgent{


    public class NetworkAgentInteraction : AgentInteraction{

    public NetworkAgentInterface networkAgent;

    override public void ActivateAgent(PlayerInteractionZone player){
        networkAgent?.Activate(player);
    }


    override public void DeactivateAgent(){
        networkAgent?.Deactivate();
    }
  
    override public void ActivateFollower(Transform leader){
        networkAgent?.LockToLeader(leader);

    }
    override public void DeactivateFollower(){
        networkAgent?.UnlockLeader();
    }

    override public void ToggleDancing(){
        networkAgent?.ToggleDancing();

    }
    override public void ActivatePairDance(){
        networkAgent?.ActivatePairDance();
    }

    override public void DeactivatePairDance(){
        networkAgent?.DeactivatePairDance();
    }
}
}

}