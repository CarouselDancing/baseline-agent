using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class HighlightGroupController : MonoBehaviour
{

    abstract public void GenerateHighlights(Transform t);

    abstract public void SetMode(bool active);

}
