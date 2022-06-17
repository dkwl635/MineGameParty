using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameNode : MonoBehaviour
{
    Transform tr;
    RawImage img;



    private void Start()
    {
        tr = transform;
        img = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (tr.position.z < 0)
        {
            tr.SetAsFirstSibling();
            img.enabled = true;
        }
        else
            img.enabled = false;

    }
}
