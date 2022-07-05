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

}
