using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerTarget : MonoBehaviour
{
    public Transform leader;
    public Vector3 offset;
    public Vector3 rotationOffset;



    // Update is called once per frame
    void Update()
    {
        var targetRotation = Quaternion.Euler(0, leader.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Euler(rotationOffset.z, rotationOffset.y, rotationOffset.z) *targetRotation;
        transform.position = leader.position + targetRotation* offset;
        
        
    }
}
