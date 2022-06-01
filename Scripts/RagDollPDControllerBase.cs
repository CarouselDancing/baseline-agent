
using UnityEngine;
using UnityEngine.Events;

namespace Carousel{
    
namespace BaselineAgent{

public abstract class RagDollPDControllerBase : MonoBehaviour
{
    public bool DebugPauseOnReset;

     public PhysicsPoseProvider animationSrc;
     public UnityEvent OnReset;
     public bool active = true;

      public abstract void OnEpisodeBegin();
}

}

}