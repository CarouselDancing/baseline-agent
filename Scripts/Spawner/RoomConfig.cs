using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carousel{

    namespace BaselineAgent{
    public class RoomConfig : MonoBehaviour
    {
        public List<AgentSpawnZone> StartZones;
        
        public List<RPMAvatarEntry> rpmAvatars =  new List<RPMAvatarEntry>(){ new RPMAvatarEntry(){name = "female", url = "https://d1a370nemizbjq.cloudfront.net/209a1bc2-efed-46c5-9dfd-edc8a1d9cbe4.glb"}};
        public List<AnimatorOverrideController> AnimationOverriders;
    
    }
}
}