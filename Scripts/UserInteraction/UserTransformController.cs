using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carousel
{
    
public class UserTransformController : MonoBehaviour
{
       
    public Transform root;
    public void MoveTo(Vector3 target){ 
        var game = MirrorGameManager.Instance;
        if (game== null) {
           Debug.Log("no instance found");
           return;
        }
        var user = game.player;
        if (user== null) {
           Debug.Log("no user found");
           return;
        }
        var delta = target - user.transform.position;

        root.position += delta;

    }

}

}