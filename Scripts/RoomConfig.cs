using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carousel{

    namespace BaselineAgent{
    public class RoomConfig : MonoBehaviour
    {
        public List<AgentSpawnZone> StartZones;
        public List<string> AvatarURLs;
        public List<AnimatorOverrideController> AnimationOverriders;
    
    }
}
}