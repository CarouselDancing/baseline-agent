using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingHighlightGroupController : HighlightGroupController
{
    public Color startColor = Color.white, endColor = Color.green;
    [Range(0, 10)]
    public float speed = 2.55f;
    public bool blinkOnStart;
    public List<BlinkingHighlight> highlights;
    
    void Start(){
        highlights = new List<BlinkingHighlight>();
        GenerateHighlights(transform);
    }

    override public void GenerateHighlights(Transform t)
    {
        
        Renderer ren = t.gameObject.GetComponent<Renderer>();
        if(ren!= null) {
            var b = t.gameObject.AddComponent<BlinkingHighlight>();
            b.speed = speed;
            b.startColor = startColor;
            b.endColor = endColor;
            b.blink = blinkOnStart;
            highlights.Add(b);

        }
        for(int i = 0; i < t.childCount; i++)
        {
            GenerateHighlights(t.GetChild(i));
        }
    }

    override public void SetMode(bool active){
        foreach(var b in highlights){
            b.blink = active;
        }
    }

}
