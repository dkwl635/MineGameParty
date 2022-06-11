using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNode : MonoBehaviour
{

    Transform tr;

    private void Start()
    {
        tr = transform;
    }

    private void Update()
    {
        if (tr.position.z < 0)
            tr.SetAsFirstSibling();
    }
}
