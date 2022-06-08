using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReadyPlayerMe;
using Carousel.BaselineAgent;
using Carousel.FigureGenerator;
using Mirror;

namespace Carousel{

abstract public class RPMGeneratorBase : NetworkBehaviour
{
    public string AvatarURL = "";
    public GameObject go;
    public AnimatorOverrideController animationController;
    public bool initiated = false;
    public bool processing = false;
    public NetworkAvatar networkAvatar;

    public bool IsOwner => isLocalPlayer;



    virtual public void Update()
    {

        if (!initiated && !processing && !IsOwner && AvatarURL != "")
        {
            processing = true;
            SetupAvatarControllerFromRPM(AvatarURL);
        }
        else if (initiated && IsOwner && AvatarURL != "")
        { //TODO replace with server side avatarmanagement
            UpdateSyncvars(AvatarURL);
        }
    }


    [Command]
    public void UpdateSyncvars(string newurl)
    {
        AvatarURL = newurl;
        ChangeClientAvatar(newurl);
    }

    [ClientRpc]
    public void ChangeClientAvatar(string newurl)
    {
        AvatarURL = newurl;
    }



    public void SetupAvatarControllerFromRPM(string url)
    {
        AvatarLoader avatarLoader = new AvatarLoader();
        Debug.Log($"Started loading avatar. [{Time.timeSinceLevelLoad:F2}]");
        processing = true;
        avatarLoader.LoadAvatar(url, OnAvatarImported, OnRPMAvatarLoaded);
    }

    public void OnAvatarImported(GameObject avatar)
    {
        go = avatar;
        avatar.transform.parent = transform;
        Debug.Log($"Avatar imported. [{Time.timeSinceLevelLoad:F2}]");
        processing = false;
    }

    public void RotateArmature(GameObject o){
        for (int i = 0; i < o.transform.childCount; i++)
        {
            var t = o.transform.GetChild(i);
            if (t.name == "Armature")
            {
                t.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                break;
            }
        }
    }

    abstract public void OnRPMAvatarLoaded(GameObject avatar, AvatarMetaData metaData = null);

}
}