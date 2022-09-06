using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


namespace Carousel{
    [Serializable]
    public class AssetDesc{
        public string name;
        public string bundleName;
        public string url;
        public bool isScene;
        public Vector3 scale = new Vector3(1,1,1);
        public List<string> scripts = new List<string>();
        public AssetBundle loadedBundle;

    }

    [Serializable]
    public class LoadingScreenUI{
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

    public class SceneLoader : MonoBehaviour{
        public LoadingScreenUI loadingScreenUI;
        public UnityEvent mainSceneLoaded = new UnityEvent();
        public bool loadAsync;
        
        public string lobbyScene = "Lobby";
        public string mainScene = "main";

        public List<AssetDesc> assetDatabase = new List<AssetDesc>(){new AssetDesc{scale= new Vector3(2f,2f,2f), name = "PlantWorld", bundleName="plantworld", url=""}};
        //public List<AssetDesc> assetDatabase = new List<AssetDesc>(){new AssetDesc{isScene=true, scale= new Vector3(2f,2f,2f), name = "ModifiedEnvironment_V1_no_logic", bundleName="testscene_plantworld", url=""}};
        public List<string> loadedAssets;

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
        

        
        IEnumerator LoadAsyncScene(string sceneName, LoadSceneMode mode)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
            yield return ShowLoadingScreen("Load Scene: "+sceneName, asyncLoad);
            
        }

        IEnumerator ShowLoadingScreen(string info, AsyncOperation operation){
            if(loadingScreenUI != null){
                yield return loadingScreenUI.Show(info, operation);
            }else{
                yield return null;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            MirrorGameManager.ShowMessage("OnSceneLoaded "+ scene.name);
            if(scene.name == mainScene){
                foreach(var a in assetDatabase){
                    if(a.url != ""){
                        StartCoroutine(GetAssetFromWeb(a));
                    }else{
                        StartCoroutine(LoadLocalAssetAsync(a));
                    }
                }
                mainSceneLoaded?.Invoke();
            } 
        }

        /// Code from Unity docs 
        public IEnumerator GetAssetFromWeb(AssetDesc desc)
        {
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(desc.url);
            yield return www.SendWebRequest();
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(www);
            if(assetBundle != null){
                //GameObject g = Instantiate(bundle.LoadAsset("assettest")) as GameObject; 
                // if you want load gameobject eg. prefab etc..... uncomment the line//////

               yield return InstantiateAssetAsync(desc, assetBundle);
                //GameObject assetPrefab = bundle.LoadAsset<GameObject>(desc.name);  
                //Instantiate(assetPrefab);
                 /*
                foreach(string sceneName in bundle.GetAllScenePaths()){
                    loadedAssets.Add(sceneName);
                    var sceneFileName = Path.GetFileNameWithoutExtension(sceneName);
                    SceneManager.LoadScene(sceneFileName, LoadSceneMode.Additive);
                }*/
            } 
        }

         void LoadLocalAsset(AssetDesc desc){
            
            if (desc.loadedBundle == null){
                var filename = Path.Combine(Application.streamingAssetsPath, desc.bundleName);
                desc.loadedBundle = AssetBundle.LoadFromFile(filename);
            }
            if (desc.loadedBundle == null) {
                Debug.LogError("Failed to load AssetBundle!");
                return;
            }
            InstantiateAsset(desc, desc.loadedBundle);
        
        }

        IEnumerator LoadLocalAssetAsync(AssetDesc desc){
            if (desc.loadedBundle == null){
                var filename = Path.Combine(Application.streamingAssetsPath, desc.bundleName);
                AssetBundleCreateRequest asyncBundleRequest = AssetBundle.LoadFromFileAsync(filename);
                yield return ShowLoadingScreen("Load Asset Bundle: "+desc.bundleName, asyncBundleRequest);
                desc.loadedBundle = asyncBundleRequest.assetBundle;
            }
            if (desc.loadedBundle == null) {
                Debug.LogError("Failed to load AssetBundle!");
                yield break;
            }
            yield return InstantiateAssetAsync(desc, desc.loadedBundle);

        }

        public void InstantiateAsset(AssetDesc desc, AssetBundle assetBundle){
            if (desc.isScene){
                foreach(string sceneName in assetBundle.GetAllScenePaths()){
                    var sceneFileName = Path.GetFileNameWithoutExtension(sceneName);
                    SceneManager.LoadScene(sceneFileName, LoadSceneMode.Additive);
                }
            }else{
        	    GameObject assetPrefab = assetBundle.LoadAsset<GameObject>(desc.name);  
                GameObject o = Instantiate(assetPrefab);
                o.transform.localScale = desc.scale;
                assetBundle.Unload(false);
                
            }
        }

         IEnumerator InstantiateAssetAsync(AssetDesc desc, AssetBundle assetBundle){
            if (desc.isScene){
                foreach(string sceneName in assetBundle.GetAllScenePaths()){
                    var sceneFileName = Path.GetFileNameWithoutExtension(sceneName);
                    yield return LoadAsyncScene(sceneFileName, LoadSceneMode.Additive);
                }
            }else{
                AssetBundleRequest assetRequest = assetBundle.LoadAssetAsync<GameObject>(desc.name);
                yield return ShowLoadingScreen("Instantiate Prefab: "+desc.name, assetRequest);
                GameObject assetPrefab = assetRequest.asset as GameObject;  
                GameObject o = Instantiate(assetPrefab);
                o.transform.localScale = desc.scale;
                assetBundle.Unload(false);
                
            }
        }


    }
}