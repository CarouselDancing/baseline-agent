using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineGroupController : HighlightGroupController
{
    public Color color = Color.green;
    public List<Outline> highlights;
    public bool active = false;
    
    void Start(){
        highlights = new List<Outline>();
        GenerateHighlights(transform);
    }

    override public void GenerateHighlights(Transform t)
    {
        
        Renderer ren = t.gameObject.GetComponent<Renderer>();
        if(ren!= null) {
            var o = t.gameObject.AddComponent<Outline>();
            o.OutlineColor = color;
            o.OutlineMode = Outline.Mode.OutlineAll;
            o.enabled = active;
            highlights.Add(o);

        }
        for(int i = 0; i < t.childCount; i++)
        {
            GenerateHighlights(t.GetChild(i));
        }
    }

    override public void SetMode(bool active){
        this.active = active;
        if (highlights == null)return;
        foreach(var o in highlights){
            o.enabled  = active;
        }
    }

}
