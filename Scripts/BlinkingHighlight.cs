using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingHighlight : MonoBehaviour
{
    public Color startColor, endColor = Color.white;
    [Range(0, 10)]
    public float speed = 0.5f;
    Renderer ren;
    public bool blink = false;
    
    
    void Awake(){
        ren = GetComponent<Renderer>();
        startColor = ren.material.color;
    }
    
     void Update()
    {
        if(blink)
        {
            blinking();
        }
    
       if(!blink){
            ren.material.color =startColor;
       }
     
    }
    
     public void blinking(){
      
        ren.material.color = Color.Lerp(startColor , endColor, Mathf.PingPong(Time.time * speed, 0.4f));

    }
}