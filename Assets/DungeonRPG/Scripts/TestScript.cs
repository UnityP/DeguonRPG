using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public List<int> intMakers=new List<int>(){0,1,2,3,4}; // 경로 생성자(PathMaker)의 리스트

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ButtonGroup]
    [GUIColor(0, 1, 0)]
    void ButtonFunction()
    {
        for (int i = intMakers.Count - 1; i >= 0; i--)
        {
            if (i == 1)
            {
                intMakers.RemoveAt(i);
                continue;
            }
            
            Debug.Log("Value of int maker:" + i);
        }
    }
}
