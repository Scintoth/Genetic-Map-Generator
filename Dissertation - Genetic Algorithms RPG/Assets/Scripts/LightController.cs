using UnityEngine;
using System.Collections;

public class LightController : MonoBehaviour
{
    public float RotationRate = 2;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(Vector3.left,RotationRate * Time.deltaTime);
	}
}
