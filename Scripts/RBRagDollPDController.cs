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
    List<ArticulationBody> _bodyParts;
    RagDoll003 _ragDollSettings;
    List<ConfigurableJoint> _motors;


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
        if(!active)return;
        
        GetMocapTargets();
    
        int i = 0;
        foreach (var m in _motors)
        {
            Quaternion targetRotation = _mocapTargets[i];
            m.targetRotation = Quaternion.Inverse(targetRotation) * _originalRotations[i];
            i++;
        }
    }


   public override void OnEpisodeBegin()
    {

        if (!_hasLazyInitialized && animationSrc != null)
        {
         
            _mocapBodyParts = animationSrc.GetComponentsInChildren<Rigidbody>().ToList();
            _mocapBodyPartsA = animationSrc.GetComponentsInChildren<ArticulationBody>().ToList();
    
            _bodyParts = GetComponentsInChildren<ArticulationBody>().ToList();
            _ragDollSettings = GetComponent<RagDoll003>();

            foreach (var body in GetComponentsInChildren<ArticulationBody>())
            {
                body.solverIterations = 255;
                body.solverVelocityIterations = 255;
            }

            _motors = GetComponentsInChildren<ConfigurableJoint>()
                .Distinct()
                .ToList();

            storeOriginalRotations();
            _hasLazyInitialized = true;
            animationSrc.ResetToIdle();
            animationSrc.CopyStatesToRB(this.gameObject);
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
 
}

}
}