using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserConnectionVisualization
{
    public LineRendererController _leftHand;
    public LineRendererController _rightHand;
    public Transform _leftTarget;
    public Transform _rightTarget;
    public bool active = false;

    public void Init(Transform leftHand, Transform rightHand){
        _leftHand = leftHand.gameObject.AddComponent<LineRendererController>();
        _rightHand = rightHand.gameObject.AddComponent<LineRendererController>();

    }

    public void Update(){
        if(!active)return;
        if(_leftTarget == null || _rightTarget == null){
            Deactivate();
            return;
        }
        _leftHand.SetPoints(_leftHand.transform.position, _leftTarget.position);
        _rightHand.SetPoints(_rightHand.transform.position, _rightTarget.position);
    }

    public void Activate(Transform leftTarget, Transform rightTarget){
        active = true;
        _leftHand.Activate();
        _rightHand.Activate();
    }

    public void Deactivate(){
        active = false;
        _leftTarget = null;
        _rightTarget = null;
        _leftHand.Deactivate();
        _rightHand.Deactivate();

    }
   
}
