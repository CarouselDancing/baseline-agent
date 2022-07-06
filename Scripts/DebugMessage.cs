using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugMessage : MonoBehaviour
{
    public Text text;

     public void Start(){
        MirrorGameManager.Instance.debugMessage = this;
    }
    public void Show(string message){
        text.text = message;
    }

}
