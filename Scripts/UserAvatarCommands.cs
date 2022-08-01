using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Carousel.BaselineAgent;


public class UserAvatarCommands : NetworkBehaviour
{
    public static int LEFT = 0;
    public static int RIGHT = 1;
    public RPMUserAvatar userAvatar;

    
    public void GrabLeft(){
        CmdGrab(0);
    }   
    public void GrabRight(){
        CmdGrab(1);
    }

    public void Grab(int side){
        CmdGrab(side);
    }
    [Command]
    void CmdGrab(int side){
     if (side == RIGHT){
            if(userAvatar.rightGrabber!=null) {
                 userAvatar.rightGrabber.GrabObject();
            }
        }else{
            if(userAvatar.leftGrabber != null) {
                userAvatar.leftGrabber.GrabObject();
            }
        }
    }   
    
    public void ReleaseLeft(){
        CmdRelease(0);
    }

    public void ReleaseRight(){
        CmdRelease(1);
    }

    public void Release(int side){
        CmdRelease(side);
    }


    [Command]
    void CmdRelease(int side){
          if (side == RIGHT){
                if(userAvatar.rightGrabber!=null) {
                     userAvatar.rightGrabber.ReleaseObject();
                }
            }else{
                if(userAvatar.leftGrabber != null) {
                     userAvatar.leftGrabber.ReleaseObject();
                }
            }
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
