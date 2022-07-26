using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Carousel.BaselineAgent;


public class UserMenu : MonoBehaviour
{
   public RPMUserAvatar _userAvatar;
   public UserAvatarCommands _userCommands;

    public void Start(){
        MirrorGameManager.Instance.userMenu = this;
    }

   public void Register(RPMUserAvatar userAvatar){
      _userAvatar = userAvatar;
      _userCommands = userAvatar.gameObject.GetComponent<UserAvatarCommands>();
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

   public void ExitGame(){
         Application.Quit();
   }
}
