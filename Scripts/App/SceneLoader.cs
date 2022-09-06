using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Events;


namespace Carousel{
   

    public class SceneLoader : MonoBehaviour{
        public UnityEvent mainSceneLoaded = new UnityEvent();
        public bool loadAsync;
        
        public string lobbyScene = "Lobby";
        public string mainScene = "main";

        public AssetDatabase assetDB;

        void Start(){

            SceneManager.sceneLoaded += OnSceneLoaded;

        }  
        
        public void LoadMainScene(){
            LoadLocalScene(mainScene);
        }

        public void LoadLobbyScene(){
            LoadLocalScene(lobbyScene);
        }

        public void LoadLocalScene(string sceneName, LoadSceneMode mode=LoadSceneMode.Single){
            if(loadAsync){
                StartCoroutine(LoadAsyncScene(sceneName, mode));
            }else{    
                SceneManager.LoadScene(sceneName, mode);
            }
        }
        
        public IEnumerator LoadAsyncScene(string sceneName, LoadSceneMode mode)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
            yield return MirrorGameManager.Instance.ShowLoadingScreen("Load Scene: "+sceneName, asyncLoad);
            
        }


        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            MirrorGameManager.ShowMessage("OnSceneLoaded "+ scene.name);
            if(scene.name == mainScene){
                var desc = assetDB.assetList[assetDB.defaultAsset];
                if(desc.isRemote){
                    StartCoroutine(assetDB.GetAssetFromWeb(desc));
                }else{
                    Debug.Log("load local asset");
                    StartCoroutine(assetDB.LoadLocalAssetAsync(desc));
                }
                mainSceneLoaded?.Invoke();
            } 
        }

    }
}