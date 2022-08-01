using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugMessage : MonoBehaviour
{
    public GameObject panel;
    public Text text;

     public void Start(){
        MirrorGameManager.Instance.debugMessage = this;
    }
    public void Write(string message){
        text.text = message;
    }    

    public void ToggleVisibility(){
        if(!panel.activeInHierarchy){
            Show();
        }else{
            Hide();
        }
    }
    
    public void Show(){
        panel.SetActive(true);
    }
       public void Hide(){
        panel.SetActive(false);
    }

}
