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
       Debug.Log("spawn agent");
       _userAvatar?.SpawnAgent();
   }   
   
   public void ActivateAgent(){
       Debug.Log("ActivateAgent");
       _userAvatar?.ActivateAgent();
   } 
   
   public void DeactivateAgent(){
       Debug.Log("ActivateAgent");
       _userAvatar?.DeactivateAgent();
   } 

   public void ActivateFollower(){
       Debug.Log("ActivateFollower");
       _userAvatar?.ActivateFollower();
   }    

   public void DeactivateFollower(){
       Debug.Log("DeactivateFollower");
       _userAvatar?.DeactivateFollower();
   } 

   public void ActivatePairDance(){
       Debug.Log("ActivatePairDance");
       _userAvatar?.ActivatePairDance();
   } 

   public void DeactivatePairDance(){
       Debug.Log("ActivatePairDance");
       _userAvatar?.DeactivatePairDance();
   } 

   public void ToggleAgentDancing(){
       Debug.Log("ToggleAgentDancing");
       _userAvatar?.ToggleAgentDancing();
   } 
   public void RemoveAgent(){
       Debug.Log("RemoveAgent");
       _userAvatar?.RemoveAgent();
   }


   public void SetHeight(){
       Debug.Log("SetHeight");
       _userAvatar?.SetHeight();
      
   }

   public void ExitGame(){
         Application.Quit();
   }
}
