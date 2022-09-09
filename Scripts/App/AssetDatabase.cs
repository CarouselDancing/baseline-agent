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
    [Serializable]
    public class AssetDesc{
        public string assetName;
        public string bundleName;
        public string url;
        public bool isRemote;
        public bool isScene;
        public Vector3 offset = new Vector3(0,0,0);
        public Vector3 scale = new Vector3(1,1,1);
        public List<string> scripts = new List<string>();
        public AssetBundle loadedBundle;

    } 
    [Serializable]
    
    public class AssetDatabase{

       public List<AssetDesc> assetList = new List<AssetDesc>(){
                                                                new AssetDesc{scale= new Vector3(2f,2f,2f), assetName = "PlantWorld", bundleName="plantworld", url=""},                                                                
                                                                };

 

  
        public int defaultAsset = 0;
        public List<string> loadedAssets;

        public static AssetDatabase Init(string text)
        {
            return JsonUtility.FromJson<AssetDatabase>(text);
        }

        /// Code from Unity docs 
        public IEnumerator GetAssetFromWeb(AssetDesc desc)
        {
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(desc.url);
            yield return www.SendWebRequest();
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(www);
            if(assetBundle != null){

               yield return InstantiateAssetAsync(desc, assetBundle);
            } 
        }

        public void LoadLocalAsset(AssetDesc desc){
            
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

        public IEnumerator LoadLocalAssetAsync(AssetDesc desc){
            if (desc.loadedBundle == null){
                var filename = Path.Combine(Application.streamingAssetsPath, desc.bundleName);
                AssetBundleCreateRequest asyncBundleRequest = AssetBundle.LoadFromFileAsync(filename);
                yield return MirrorGameManager.Instance.ShowLoadingScreen("Load Asset Bundle: "+desc.bundleName, asyncBundleRequest);
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
        	    GameObject assetPrefab = assetBundle.LoadAsset<GameObject>(desc.assetName);  
                GameObject o = GameObject.Instantiate(assetPrefab);
                o.transform.localScale = desc.scale;
                o.transform.position += desc.offset;
                assetBundle.Unload(false);
                
            }
        }

        public IEnumerator InstantiateAssetAsync(AssetDesc desc, AssetBundle assetBundle){
            if (desc.isScene){
                foreach(string sceneName in assetBundle.GetAllScenePaths()){
                    var sceneFileName = Path.GetFileNameWithoutExtension(sceneName);
                    yield return MirrorGameManager.Instance.sceneLoader.LoadAsyncScene(sceneFileName, LoadSceneMode.Additive);
                }
            }else{
                AssetBundleRequest assetRequest = assetBundle.LoadAssetAsync<GameObject>(desc.assetName);
                yield return MirrorGameManager.Instance.ShowLoadingScreen("Instantiate Asset: "+desc.assetName, assetRequest);
                GameObject assetPrefab = assetRequest.asset as GameObject;  
                GameObject o = GameObject.Instantiate(assetPrefab);
                o.transform.localScale = desc.scale;
                o.transform.position += desc.offset;
                assetBundle.Unload(false) ;
                 
            }
        }


        public void CompileScript(AssetBundle assetBundle, string filename){
        }


    }
}