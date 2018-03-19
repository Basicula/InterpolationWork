using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMove : MonoBehaviour
{
    private Camera camera;
    private GameObject line;
    // Use this for initialization
    void Start()
    {
        camera = GameObject.Find("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
