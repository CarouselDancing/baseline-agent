

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Carousel{
     namespace BaselineAgent{

    public class GlobalAgentGameState : GlobalGameState{

    public RoomConfig roomConfig;

     private GlobalAgentGameState()
    {
        Load();
        roomConfig =  (RoomConfig)GameObject.FindObjectsOfTypeAll(typeof(RoomConfig)).FirstOrDefault();
    }
        
    public static GlobalAgentGameState GetInstance()
    {
        if (instance == null)
        {
            instance = new GlobalAgentGameState();
        }
        return (GlobalAgentGameState)instance;
    }
}

     }
}


