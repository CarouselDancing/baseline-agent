using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Carousel{
    
namespace BaselineAgent{

public class AgentSpawnZone : MonoBehaviour
{
        public Vector3 GetRandomStartPosition()
    {
        var bc = GetComponent<BoxCollider>();
        float xo = Random.Range(-bc.size[0],bc.size[0])/2f;
        float yo = -bc.size[1] /2;
        float zo = Random.Range(-bc.size[2],bc.size[2])/2f;
        return transform.position + new Vector3(xo, yo, zo);
    }

}

}
}

