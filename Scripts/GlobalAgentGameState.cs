

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Carousel{
     namespace BaselineAgent{

    public class GlobalAgentGameState {
        public ClientConfig config;
        public RoomConfig roomConfig;
        protected static GlobalAgentGameState instance;

        private GlobalAgentGameState()
        {
            Load();
            roomConfig =  (RoomConfig)GameObject.FindObjectsOfTypeAll(typeof(RoomConfig)).FirstOrDefault();
        }
            

        protected void Load()
        {
            var configText = Resources.Load<TextAsset>("config").text;
            config = JsonUtility.FromJson<ClientConfig>(configText);
            Debug.Log(config.ToString());

        }

        public static GlobalAgentGameState GetInstance()
        {
            if (instance == null)
            {
                instance = new GlobalAgentGameState();
            }
            return instance;
        }
    }

     }
}


