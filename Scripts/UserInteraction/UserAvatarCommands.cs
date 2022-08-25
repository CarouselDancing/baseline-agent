using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Carousel.BaselineAgent;


public class UserAvatarCommands : NetworkBehaviour
{
    public RPMUserAvatar userAvatar;

    
    public void GrabLeftHand(){
        GrabHand(0);
    }   
    public void GrabRightHand(){
        GrabHand(1);
    }

    public void GrabHand(int side){
        CmdGrabHand(side);
    }
    [Command]
    void CmdGrabHand(int side){
        userAvatar.GrabHand(side);
    }   
    
    public void ReleaseLeftHand(){
        ReleaseHand(0);
    }

    public void ReleaseRightHand(){
        ReleaseHand(1);
    }

    public void ReleaseHand(int side){
        CmdReleaseHand(side);
    }


    [Command]
    void CmdReleaseHand(int side){
        userAvatar.ReleaseHand(side);
    }


    public void SpawnAgent()
    {
        CmdSpawnAgent();
    }


    [Command]
    void CmdSpawnAgent()
    {
       RoomManager.Instance.SpawnAgent();
    }


    public void ToggleFollower(){
        CmdToggleFollower();
    }

    [Command]
    void CmdToggleFollower(){
        userAvatar.interactionZone.ToggleFollower();
    } 
    
    public void ToggleMirror(){
        CmdToggleMirror();
    }

    [Command]
    void CmdToggleMirror(){
        userAvatar.interactionZone.ToggleMirror();
    }

    public void ActivateAgent(){
        CmdActivateAgent();
    }
    
    [Command]
    void CmdActivateAgent(){
        userAvatar.interactionZone.ActivateAgent();
    }

    public void DeactivateAgent(){
        CmdDeactivateAgent();
    }

    [Command]
    void CmdDeactivateAgent(){
        userAvatar.interactionZone.DeactivateAgent();
    }
    public void ActivateFollower(){
        CmdActivateFollower();
    }

    [Command]
    void CmdActivateFollower(){
        userAvatar.interactionZone.ActivateFollower();
    }
    public void DeactivateFollower(){
        CmdActivateFollower();
    }

    [Command]
    void CmdDeactivateFollower(){
        userAvatar.interactionZone.DeactivateFollower();
        
    } 
    public void ToggleAgentDancing(){
        CmdToggleAgentDancing();
    }

    [Command]
    void CmdToggleAgentDancing(){
        userAvatar.interactionZone.ToggleDancing();
        
    }

    public void ActivatePairDance(){
        CmdActivatePairDance();
    }

    [Command]
    void CmdActivatePairDance(){
        userAvatar.interactionZone.ActivatePairDance();
        
    }

    public void DeactivatePairDance(){
        CmdDeactivatePairDance();
    }

    [Command]
    void CmdDeactivatePairDance(){
        userAvatar.interactionZone.DeactivatePairDance();
        
    }

    public void RemoveAgent(){
        CmdRemoveAgent();
    }

    [Command]
    void CmdRemoveAgent(){
        userAvatar.interactionZone.RemoveAgent();
        
    }

    public void SetHeight(){
        userAvatar.SetHeight();
    }


}
