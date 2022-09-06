using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


namespace Carousel{


    [Serializable]
    public class LoadingScreen{
        public GameObject gameObject;
        public Slider progessbar;
        public Text infoText;

        
        public IEnumerator Show(string info, AsyncOperation operation){
            if(infoText!= null)infoText.text = info;
            gameObject.SetActive(true);
            // asyncLoad.allowSceneActivation = false;
            // Wait until the asynchronous scene fully loads
            while (!operation.isDone)
            {
                float loading = Mathf.Clamp01(operation.progress / .9f);
                if(progessbar != null) progessbar.value = loading;
                yield return null;
            } 
            gameObject.SetActive(false);
        }

    }
}