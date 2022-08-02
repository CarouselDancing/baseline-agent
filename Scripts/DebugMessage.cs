using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugMessage : MonoBehaviour
{
    public GameObject panel;
    public GameObject contentObject;
    //public Text text;
    public List<string> messages;
    public List<GameObject> messgeObjects;
    public int maxCount = 10;
    public GameObject entryPrefab;

     public void Start(){
        messgeObjects = new List<GameObject>();
        messages = new List<string>();
        MirrorGameManager.Instance.debugMessage = this;
    }
    public void Write(string message){
        messages.Add(message);
        if(messages.Count > maxCount){
            messages.RemoveAt(0);
        }
        UpdateMessages();
    }    

    public void UpdateMessages(){
        foreach(var mo in messgeObjects){
            Destroy(mo);
        }
        messgeObjects = new List<GameObject>();
        for (int i = messages.Count-1; i >=0; i--){
            
            // var mo = new GameObject(count.ToString());
            // mo.transform.parent = contentObject.transform;
            var mo = GameObject.Instantiate(entryPrefab, contentObject.transform);
            var t = mo.GetComponentInChildren<Text>();
            t.text = messages[i];
            messgeObjects.Add(mo);
        }
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
