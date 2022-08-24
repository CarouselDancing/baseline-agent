using UnityEngine;

namespace Carousel{
    
namespace BaselineAgent{
public class PairDanceFollower : MonoBehaviour
{

   
    public Transform leader;
    public Vector3 offset;
    public Quaternion offsetRotation;

    virtual public void ActivatePairDance(PlayerControllerBase player){
        leader = player.transform;
    }

    virtual public void DeactivatePairDance()
    {
        leader = null;

    }
    

}
}
}