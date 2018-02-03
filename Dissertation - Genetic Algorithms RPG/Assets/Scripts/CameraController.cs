using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float baseSpeed = 3f;
    public float currentSpeed = 3f;
    public float acceleration = 0.1f;
    public bool useAcceleration;
    public bool boostSpeed;

	// Use this for initialization
	public void Begin ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	public void CameraControllerUpdate ()
    {
        GetAdditionalKeys();

        Accelerate();
        
        GetMovementKeys();
    }

    void GetAdditionalKeys()
    {
        boostSpeed = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            useAcceleration = !useAcceleration;
        }
    }

    void Accelerate()
    {
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && useAcceleration)
        {
            if (currentSpeed > maxSpeed)
            {
                currentSpeed += acceleration;
            }
        }
        else
        {
            currentSpeed = baseSpeed;
        }
    }

    void GetMovementKeys()
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (boostSpeed)
            {
                transform.Translate(transform.forward * maxSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(transform.forward * currentSpeed * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (boostSpeed)
            {
                transform.Translate(-transform.right * maxSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(-transform.right * currentSpeed * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (boostSpeed)
            {
                transform.Translate(transform.right * maxSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(transform.right * currentSpeed * Time.deltaTime);
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (boostSpeed)
            {
                transform.Translate(-transform.forward * maxSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(-transform.forward * currentSpeed * Time.deltaTime);
            }
        }
    }
}
