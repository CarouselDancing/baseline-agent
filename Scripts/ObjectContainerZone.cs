using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Carousel
{
    

public class ObjectCollectionZone : MonoBehaviour
{

    public List<CollectableObject> collection;
     void  OnTriggerEnter (Collider other){
        var o = other.GetComponent<CollectableObject>();
        if(o != null && !collection.Contains(o)) {
            collection.Add(o);
        }
    }

    void  OnTriggerExit (Collider other){
        var o = other.GetComponent<CollectableObject>();
        if(collection.Contains(o)) {
            collection.Remove(o);
        }
    }


}
}