using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carousel{
    
namespace BaselineAgent{

public class RBRagDollPDController : RagDollPDControllerBase
{

    List<Rigidbody> _mocapBodyParts;
    List<ArticulationBody> _mocapBodyPartsA;
    List<Rigidbody> _bodyParts;
    RagDoll003 _ragDollSettings;
    List<ConfigurableJoint> _motors;
    Rigidbody root;


    bool _hasLazyInitialized;
    public Quaternion[] _mocapTargets;
    public Quaternion[] _originalRotations;

    void Start(){
    }

    void Update()
    {
    
        
        if (!_hasLazyInitialized)
        {
            OnEpisodeBegin();
            return;
        }
        if(activateRootRepair)HandleBrokenRoot();
        switch (mode){
            case PDControllerMode.OFF:
                CopyBodyStates();
                break;
            case PDControllerMode.FULL:
                ApplyPDTargets();
                break;
            case PDControllerMode.UPPER_BODY:
                CopyLowerBodyStates();
                ApplyUpperBodyPDTargets();
                break;
        };
    }

    public void ApplyPDTargets(){

        GetMocapTargets();
    
        int i = 0;
        foreach (var m in _motors)
        {
            Quaternion targetRotation = _mocapTargets[i];
            m.targetRotation = Quaternion.Inverse(targetRotation) * _originalRotations[i];
            i++;
        }
    }


    public void ApplyUpperBodyPDTargets(){

        GetMocapTargets();
    
        int i = 0;
        foreach (var m in _motors)
        {
            if (bodyTypes[m.name] != BodyType.UPPER)
                continue;
            Quaternion targetRotation = _mocapTargets[i];
            m.targetRotation = Quaternion.Inverse(targetRotation) * _originalRotations[i];
            i++;
        }
    }


    public void HandleBrokenRoot(){
        if(delayedActivation && mode == PDControllerMode.OFF){
            CopyBodyStates();
            ActivateUpperBody();
            delayedActivation = false;
        }else if(createRootJoint){
            float rootDistance = (kinematicReferenceRoot.position - root.transform.position).magnitude;
            if (rootDistance > maximumRootDistance){
                Deactivate();
                delayedActivation = true;
            }
        }
    }

   public override void OnEpisodeBegin()
    {

        if (!_hasLazyInitialized && animationSrc != null)
        {
         
            _mocapBodyParts = animationSrc.GetComponentsInChildren<Rigidbody>().ToList();
            _mocapBodyPartsA = animationSrc.GetComponentsInChildren<ArticulationBody>().ToList();
    
            if (_mocapBodyParts.Count > 0) {
                Rigidbody mocapBody = _mocapBodyParts.First(x => x.name == animationSrc.rootName);
                kinematicReferenceRoot = mocapBody.transform;
            }
            else
            {
                ArticulationBody mocapBody = _mocapBodyPartsA.First(x => x.name ==animationSrc.rootName);
                kinematicReferenceRoot = mocapBody.transform;
            }
            _bodyParts = GetComponentsInChildren<Rigidbody>().ToList();
            bodyTypes = new Dictionary<string, BodyType>();
            bodyMap = new List<BodyMap>();
            
            foreach(var b in _bodyParts){
                b.solverIterations = solverIterations;
                b.solverVelocityIterations = solverIterations;
                BodyType bt = BodyType.LOWER;
                if(b.name == kinematicReferenceRoot.name){
                    bt = BodyType.ROOT;
                    root = b;
                }else if(upperBodyNames.Contains(b.name)){
                    bt = BodyType.UPPER;
                    Debug.Log("upper "+b.name);
                }else{
                    Debug.Log("lower "+b.name + upperBodyNames.Count().ToString());
                }
               
                bodyTypes[b.name] = bt;
                var src = animationSrc.GetComponentsInChildren<Transform>().First(x => x.name == b.name);
                bodyMap.Add(new BodyMap(){dst= b.transform, src = src});
            }
            bodyMap.Add(new BodyMap(){dst= root.transform, src = kinematicReferenceRoot.transform});
            _ragDollSettings = GetComponent<RagDoll003>();


            _motors = GetComponentsInChildren<ConfigurableJoint>()
                .Distinct()
                .ToList();

            storeOriginalRotations();
            _hasLazyInitialized = true;
            animationSrc.ResetToIdle();
            if(!createRootJoint){
                animationSrc.CopyStatesToRB(this.gameObject);
            }
            if (mode != PDControllerMode.FULL){
               deactivateBodies();
            }
            else if (mode == PDControllerMode.UPPER_BODY){
                activateUpperBodies();
            }

            if(createRootJoint){
                CopyBodyStates();
                CreateRootJoint();
            }
            
        }

     
        OnReset?.Invoke();

#if UNITY_EDITOR
        if (DebugPauseOnReset)
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
#endif	      
    }
    void storeOriginalRotations()
    {
        _originalRotations = _motors.SelectMany(x => {
                List<Quaternion> list = new List<Quaternion>();
                    list.Add(Quaternion.identity);
                return list.ToArray();
            }).ToArray();
        int i = 0;
        foreach (var joint in _motors)
        {
            Transform bodyTransform = joint.transform;
            Quaternion localRot = bodyTransform.localRotation;
            if (bodyTransform.parent.GetComponent<Rigidbody>() == null)
            {
                localRot = bodyTransform.parent.localRotation * localRot;
            }
            _originalRotations[i] =localRot;
             i++;
        }
    }    

    Quaternion[] GetMocapTargets()
    {
        if (_mocapTargets == null)
        {
            _mocapTargets = _motors.SelectMany(x => {
                    List<Quaternion> list = new List<Quaternion>();
                        list.Add(Quaternion.identity);
                    return list.ToArray();
                })
                .ToArray();
        }
        int i = 0;
        foreach (var joint in _motors)
        {
            Quaternion localRot;
            Transform mocapBodyTransform;
            if (_mocapBodyParts.Count > 0) {
                Rigidbody mocapBody = _mocapBodyParts.First(x => x.name == joint.name);
                mocapBodyTransform = mocapBody.transform;
                localRot = mocapBodyTransform.localRotation;
                if (mocapBodyTransform.parent.GetComponent<Rigidbody>() == null)
                {
                    localRot = mocapBodyTransform.parent.localRotation * localRot;
                }
            }
            else
            {
                ArticulationBody mocapBody = _mocapBodyPartsA.First(x => x.name == joint.name);
                mocapBodyTransform = mocapBody.transform;
                localRot = mocapBodyTransform.localRotation;
            }
            _mocapTargets[i] = localRot;
             i++;
            
        }
        return _mocapTargets;
    }    
 
 
    override public void CreateRootJoint(){
        kinematicReferenceRoot = null;
        foreach (var m in bodyMap){
            if (bodyTypes[m.dst.name] == BodyType.ROOT){
                kinematicReferenceRoot = m.src;
                break;
            }
        }
        if(kinematicReferenceRoot == null)return;
        rootJoint = kinematicReferenceRoot.gameObject.AddComponent<ConfigurableJoint>();
        rootJoint.angularXMotion = ConfigurableJointMotion.Locked;
        rootJoint.angularYMotion = ConfigurableJointMotion.Locked;
        rootJoint.angularZMotion = ConfigurableJointMotion.Locked;
        rootJoint.xMotion = ConfigurableJointMotion.Locked;
        rootJoint.yMotion = ConfigurableJointMotion.Locked;
        rootJoint.zMotion = ConfigurableJointMotion.Locked;
        rootJoint.enablePreprocessing = enableRootJointPreprocessing;
        rootJoint.connectedBody = root;
    }


    override public void Deactivate(){
        mode = PDControllerMode.OFF;
        if(!_hasLazyInitialized) return;
        deactivateBodies();
        var stateController = animationSrc.GetComponent<AnimStateController>();
        if(stateController!= null)stateController.applyRootMotion = true;
    }

    override public void Activate(){
        mode = PDControllerMode.FULL;
        if(!_hasLazyInitialized) return;
        activateBodies();
        if(rootJoint != null) {
            rootJoint.connectedBody = root;
        }
    }

    void activateBodies(){
        foreach (var b in _bodyParts)
        {
            b.isKinematic = false;
            b.solverIterations = solverIterations;
            b.solverVelocityIterations = solverIterations;
        }
     }
     void deactivateBodies(){
        foreach (var b in _bodyParts)
        {
            b.isKinematic = true;
        }
     }

     void activateUpperBodies(){
       foreach (var b in _bodyParts)
        {
            if(bodyTypes[b.name] == BodyType.UPPER || bodyTypes[b.name] == BodyType.ROOT){
                b.isKinematic = false;
                b.solverIterations = solverIterations;
                b.solverVelocityIterations = solverIterations;
            }
        }
     }
     
     public void ActivateUpperBody(){
        mode = PDControllerMode.UPPER_BODY;
        if(!_hasLazyInitialized) return;
        activateUpperBodies();
        if(rootJoint != null) {
            rootJoint.connectedBody = root;
        }
    }

    
    public void CopyLowerBodyStates(){
        foreach (var m in bodyMap){
            switch (bodyTypes[m.dst.name]){
                /*case BodyType.ROOT:
                   //  root.TeleportRoot( m.src.position, m.src.rotation);
                    m.dst.transform.position = m.src.position;
                    m.dst.transform.rotation = m.src.rotation;
                    break;*/
                case BodyType.LOWER:
                    m.dst.transform.position = m.src.position;
                    m.dst.transform.rotation = m.src.rotation;
                    break;
            }
        }
    }


}

}
}