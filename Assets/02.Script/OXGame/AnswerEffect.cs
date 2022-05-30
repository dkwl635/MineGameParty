using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnswerEffect : MonoBehaviour
{
    TextMeshProUGUI text;

    private int count = 4;
    private float timer = 0.0f;

    
    private void Update()
    {
        if (count <= 0)
            return;

        timer += Time.deltaTime;
        
        if(timer >= 0.2f)
        {
            timer = 0.0f;
            text.color = text.color.a == 0 ? Color.blue : Color.clear;
            count--;
        }

    }

    private void Awake()
    { 
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        count = 4;
        timer = 0.0f;
    }
}
