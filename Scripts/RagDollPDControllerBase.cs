using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Carousel{
    
namespace BaselineAgent{

public enum BodyType{
    ROOT,
    LOWER,
    UPPER
}

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
    protected Dictionary<string, BodyType> bodyTypes;
     
    public bool alignReferenceRoot = true;
    public ConfigurableJoint rootJoint;
    public bool createRootJoint = false;
    protected Transform kinematicReferenceRoot;

    public float maximumRootDistance = 0.9f;
    public bool activateRootRepair = true;

    public int solverIterations = 255;
    public bool delayedActivation = false;

    
    public void CopyBodyStates(){
        foreach (var m in bodyMap){
            m.dst.transform.position = m.src.position;
            m.dst.transform.rotation = m.src.rotation;
        }
     }

    abstract public void OnEpisodeBegin();
 
    abstract public void CreateRootJoint();

    public void RemoveJoint(){
        if(rootJoint != null)DestroyImmediate(rootJoint);
        rootJoint = null;

    }
    abstract public void Activate();

    abstract public void Deactivate();

    }

}

}