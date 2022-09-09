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
        var size = bc.bounds.size;
        float xo = Random.Range(-size[0],size[0])/2f;
        float yo = -size[1] /2;
        float zo = Random.Range(-size[2],size[2])/2f;
        var pos = new Vector3(xo, yo, zo);
        return transform.position + pos;
    }

}

}
}

