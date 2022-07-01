using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Carousel{
    
namespace BaselineAgent{

public enum PDControllerMode{
    OFF,
    FULL,
    UPPER_BODY
}

public abstract class RagDollPDControllerBase : MonoBehaviour
{
    [Serializable]
    public class BodyMap{
        public Transform src;
        public Transform dst;
    }

    public bool DebugPauseOnReset;
    public PhysicsPoseProvider animationSrc;
    public UnityEvent OnReset;
    public PDControllerMode mode = PDControllerMode.FULL;
    public bool IsActive{  get { return mode != PDControllerMode.OFF; }}  
    public List<BodyMap> bodyMap;
    public List<string> upperBodyNames;
     
     public void CopyBodyStates(){
        foreach (var m in bodyMap){
            m.dst.transform.position = m.src.position;
            m.dst.transform.rotation = m.src.rotation;
        }
     }

      public abstract void OnEpisodeBegin();
}

}

}