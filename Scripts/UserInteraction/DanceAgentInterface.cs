using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carousel{
    
namespace BaselineAgent{

public abstract class DanceAgentInterface : MonoBehaviour
{
  
    public virtual bool IsFollowing{  get { return false; }}
    public virtual bool IsMirroring{  get { return false; }}
    abstract public void Activate(PlayerInteractionZone player);
    abstract public void Deactivate();
    abstract public void LockToLeader(Transform t);
    abstract public void UnlockLeader();
    abstract public void Reset();
    abstract public void ToggleDancing();
    abstract public void ActivatePairDance();
    abstract public void DeactivatePairDance();

}
}
}