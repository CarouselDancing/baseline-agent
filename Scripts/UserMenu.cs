using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Carousel.BaselineAgent;


public class UserMenu : MonoBehaviour
{
   public RPMUserAvatar _userAvatar;


/*
   public void Update(){
      if(_userAvatar != null)return;
      var avatars = GameObject.FindObjectsOfTypeAll(typeof(RPMUserAvatar));
      foreach(RPMUserAvatar a in avatars){
          if( a!= null && a.initiated && a.IsOwner) Register(a);
      }  
   }*/

   public void Register(RPMUserAvatar userAvatar){
      _userAvatar = userAvatar;
   }

   public void SpawnAgent(){
        MirrorGameManager.ShowMessage("Spawn Agent");
       _userAvatar?.SpawnAgent();
   }   
   
   public void ActivateAgent(){
        MirrorGameManager.ShowMessage("ActivateAgent");
       _userAvatar?.ActivateAgent();
   } 
   
   public void DeactivateAgent(){
        MirrorGameManager.ShowMessage("DeactivateAgent");
       _userAvatar?.DeactivateAgent();
   }   
   
   public void ToggleFollower(){
        MirrorGameManager.ShowMessage("ToggleFollower");
       _userAvatar?.ToggleFollower();
   }    

   public void ActivateFollower(){
        MirrorGameManager.ShowMessage("ActivateFollower");
       _userAvatar?.ActivateFollower();
   }    

   public void DeactivateFollower(){
        MirrorGameManager.ShowMessage("DeactivateFollower");
       _userAvatar?.DeactivateFollower();
   } 

   public void ActivatePairDance(){
        MirrorGameManager.ShowMessage("ActivatePairDance");
       _userAvatar?.ActivatePairDance();
   } 

   public void DeactivatePairDance(){
        MirrorGameManager.ShowMessage("DeactivatePairDance");
       _userAvatar?.DeactivatePairDance();
   } 

   public void ToggleAgentDancing(){
        MirrorGameManager.ShowMessage("ToggleAgentDancing");
       _userAvatar?.ToggleAgentDancing();
   } 
   public void RemoveAgent(){
        MirrorGameManager.ShowMessage("RemoveAgent");
       _userAvatar?.RemoveAgent();
   }


   public void SetHeight(){
        MirrorGameManager.ShowMessage("SetHeight");
       _userAvatar?.SetHeight();
      
   }

   public void ExitGame(){
         Application.Quit();
   }
}
