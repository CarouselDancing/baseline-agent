using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Carousel{
    
namespace BaselineAgent{



public class RBGrabber : MonoBehaviour
 {
     public enum Side{
        LEFT,
        RIGHT
    }
    public Side side;
    public GameObject parent;
    public Rigidbody grabber;
    public GameObject grabbableObject;
    public ConfigurableJoint joint;
    LineRenderer lineRenderer;
    public float width = 0.1f;
    List<Vector3> positions;
    public float grabberRadius = 0.2f;
    public string layerName = "marathon";
    public int layer;

    public GameObject desiredGrabTarget;
    

     void Start()
    {
        parent = transform.parent.gameObject;
        layer = LayerMask.NameToLayer(layerName);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.enabled = false;
        positions = new List<Vector3>();
        positions.Add(Vector3.zero);
        positions.Add(Vector3.zero);
        var s = gameObject.GetComponent<SphereCollider>();
        s.radius = grabberRadius;
        

    }

    public void SetGrabbableObject(GameObject o){
        if(o == grabber || o == parent)return;
        grabbableObject = o;
        if (grabbableObject == desiredGrabTarget){
           GrabDesiredObject();
        }
        //TODO check exact trigger press here
    }


    public void RemoveGrabbableObject(){
        grabbableObject = null;
    }

    
     void  OnTriggerEnter(Collider other){
        if (other.gameObject.layer != layer) return;
        SetGrabbableObject(other.gameObject);
    }

    void  OnTriggerExit(Collider other){
        if (other.gameObject.layer != layer) return;
        if(other.gameObject == grabbableObject) {
            grabbableObject = null;
        }
    }
    public void WaitForObject(Transform t){
        desiredGrabTarget = t.gameObject;
        Debug.Log("WaitForObject "+ name+ " " + desiredGrabTarget.name);

    }
    public void GrabDesiredObject(){
        if (desiredGrabTarget == null || IsConnected()) return;
        Debug.Log("GrabDesiredObject "+ name+ " " + desiredGrabTarget.name);
        Connect(desiredGrabTarget);
    } 


    public void GrabObject(){
        if (grabbableObject == null || IsConnected()) return;
        Debug.Log("GrabObject "+ name+ " " + grabbableObject.name);
        Connect(grabbableObject);
    } 


    public void ReleaseObject(){
        Debug.Log("ReleaseObject");
        Disconnect();
        desiredGrabTarget= null;
    }


    void FixedUpdate()
    {
        //check if we can to grab a desired object that is approaching, e.g. by using ik
        if(!IsConnected()  && desiredGrabTarget!= null){
            float distanceToTarget = (desiredGrabTarget.transform.position - transform.position).magnitude;
            if(distanceToTarget > grabberRadius) return;
            GrabDesiredObject();
        }
    }

    public void Connect(GameObject o){
        var ab = o.GetComponent<ArticulationBody>();
        var rb = o.GetComponent<Rigidbody>();

        joint = grabber.gameObject.AddComponent<ConfigurableJoint>();
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        if(ab != null) {
            Debug.Log("connect "+ name+ " " + ab.name);
            joint.connectedArticulationBody = ab;
        }else if(rb != null){
            
            Debug.Log("connect "+ name+ " " + rb.name);
            joint.connectedBody = rb;
            
        }
    }


    public void Disconnect(){
        if(joint != null){
            joint.connectedArticulationBody = null;
            joint.connectedBody = null;
            if(joint != null)DestroyImmediate(joint);
            joint = null;
        }
    }

    public bool  IsConnected(){
        return joint != null && (joint.connectedArticulationBody != null || joint.connectedBody != null );
    }
}

}
}