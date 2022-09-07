using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Carousel.BaselineAgent;
using UnityEngine.UI;

public class UserMenu : MonoBehaviour
{
   public List<Selectable> agentElements;
   public List<Selectable> avatarElements;
   public RPMUserAvatar _userAvatar;
   public UserAvatarCommands _userCommands;
   public bool isAgentInteractable = false;
   public bool isAvatarInteractable = false;


    public void Start(){
        MirrorGameManager.Instance.userMenu = this;
    }

   public void Register(RPMUserAvatar userAvatar){
    _userAvatar = userAvatar;
    _userCommands = userAvatar.gameObject.GetComponent<UserAvatarCommands>();
    SetAvatarInteractability(true);

   }

   public void SpawnAgent(){
        MirrorGameManager.ShowMessage("Spawn Agent");
       _userCommands?.SpawnAgent();
   }   
   
   public void ActivateAgent(){
        MirrorGameManager.ShowMessage("ActivateAgent");
       _userCommands?.ActivateAgent();
   } 
   
   public void DeactivateAgent(){
        MirrorGameManager.ShowMessage("DeactivateAgent");
       _userCommands?.DeactivateAgent();
   }   
   
   public void ToggleFollower(){
        MirrorGameManager.ShowMessage("ToggleFollower");
       _userCommands?.ToggleFollower();
   }    

   public void ToggleMirror(){
        MirrorGameManager.ShowMessage("ToggleMirror");
       _userCommands?.ToggleMirror();
   }    

   public void ActivateFollower(){
        MirrorGameManager.ShowMessage("ActivateFollower");
       _userCommands?.ActivateFollower();
   }    

   public void DeactivateFollower(){
        MirrorGameManager.ShowMessage("DeactivateFollower");
       _userCommands?.DeactivateFollower();
   } 

   public void ActivatePairDance(){
        MirrorGameManager.ShowMessage("ActivatePairDance");
       _userCommands?.ActivatePairDance();
   } 

   public void DeactivatePairDance(){
        MirrorGameManager.ShowMessage("DeactivatePairDance");
       _userCommands?.DeactivatePairDance();
   } 

   public void ChangeDanceStyle(){
        MirrorGameManager.ShowMessage("ChangeDanceStyle");
       _userCommands?.ChangeDanceStyle();
   } 

   public void ToggleAgentDancing(){
        MirrorGameManager.ShowMessage("ToggleAgentDancing");
       _userCommands?.ToggleAgentDancing();
   } 

   public void RemoveAgent(){
        MirrorGameManager.ShowMessage("RemoveAgent");
       _userCommands?.RemoveAgent();
   }


   public void SetHeight(){
        MirrorGameManager.ShowMessage("SetHeight");
       _userAvatar?.SetHeight();
   } 
   
   
   public void MoveUp(){
        MirrorGameManager.ShowMessage("MoveUp");
       _userAvatar?.MoveUp();
   } 
   
   public void MoveDown(){
        MirrorGameManager.ShowMessage("MoveDown");
       _userAvatar?.MoveDown();
   }

   public void ExitToLobby(){
        MirrorGameManager.Instance.OpenMainMenu();
   }

   public void SetAgentInteractability(bool isActive){
    isAgentInteractable = isActive;
    foreach(var e in agentElements){
        e.interactable  = isAgentInteractable;
    }
   }   
   public void SetAvatarInteractability(bool isActive){
    isAvatarInteractable = isActive;
    foreach(var e in avatarElements){
        e.interactable  = isAvatarInteractable;
    }
   }

   public void ToggleConsole(){
         MirrorGameManager.ToggleConsole();
   }
}
