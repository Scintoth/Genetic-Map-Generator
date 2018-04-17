using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    public static WaterController WC;

    public void Awake()
    {
        WC = this;
    }

    public static void SetHeight(float height)
    {
        var currentPosition = WC.transform.position;
        currentPosition.y = height;
        WC.transform.position = currentPosition;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
