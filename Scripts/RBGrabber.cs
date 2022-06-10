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
    public Rigidbody grabber;
    public Rigidbody grabbableObject;
    public ConfigurableJoint joint;
    LineRenderer lineRenderer;
    public float width = 0.1f;
    List<Vector3> positions;
    public float grabberRadius = 0.1f;
    

     void Start()
    {
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

    public void SetGrabbableObject(Rigidbody rb){
        if(rb != grabber){
            grabbableObject = rb;
            Debug.Log("grab "+ name+ " " + rb.name);
        }else{
            Debug.Log("ignore grab "+ name+ " " + rb.name);
        }
        
        //TODO check exact trigger press here
    }

    public void RemoveGrabbableObject(){
        grabbableObject = null;
    }

    
     void  OnTriggerEnter (Collider other){
        var rb = other.GetComponent<Rigidbody>();
        if(rb != null) {
            SetGrabbableObject(rb);
        }
    }

    void  OnTriggerExit (Collider other){
        var rb = other.GetComponent<Rigidbody>();
        if(rb == grabbableObject) {
            grabbableObject = null;
        }
    }

    public void GrabObject(){
        Debug.Log("GrabObject");
        if (grabbableObject == null) return;
        CreateJoint();
        joint.connectedBody = grabber;
        lineRenderer.enabled = true;
        
    } 
    public void ReleaseObject(){
        Debug.Log("ReleaseObject");
        RemoveJoint();
    }

      void Update()
    {
        if(!lineRenderer.enabled || joint == null ) return;
        positions[0] = transform.position;
        positions[1] = joint.connectedBody.transform.position;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

      public void CreateJoint(){
        joint = grabbableObject.gameObject.AddComponent<ConfigurableJoint>();
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    }

     public void RemoveJoint(){
        if(joint != null)DestroyImmediate(joint);
        joint = null;

    }
}

}
}