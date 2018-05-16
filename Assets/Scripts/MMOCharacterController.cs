using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMOCharacterController : MonoBehaviour {

    public Transform playerCamera;
    public Transform character;
    public Transform centerPoint;


    public float mouseYPosition;

    private float mousex, mousey;

    public float mouseSensitivity = 10f;
    public float moveSpeed = 5f;

    public float zoom;

    private float zoomSpeed = 2;
    public float zoomMax = -10f;
    public float zoomMin = -2f;

    public float rotationSpeed;
    public float lift;
    public float maxHeight;
    

    public float climbSpeed;

    public float maxForwardSpeed;

    public AnimationCurve rollCurve;

    protected bool dead;
    private float gravity = 9.8f;

    private float vSpeed = 0;

    private Quaternion lastView = Quaternion.identity;


    // Use this for initialization
    void Start () {
        dead = false;
        zoom = -3;

	}
	
	// Update is called once per frame
	void Update () {
        if (!dead)
        {
            zoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

            if (zoom > zoomMin)
            {
                zoom = zoomMin;
            }
            else if (zoom < zoomMax)
            {
                zoom = zoomMax;
            }



            Cursor.visible = false;

            if (Input.GetMouseButton(1))
            {

                mousex += Input.GetAxis("Mouse X") * mouseSensitivity;
                mousey -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            }
            else if (Input.GetMouseButton(0))
            {
                mousex += Input.GetAxis("Mouse X") * mouseSensitivity;
                mousey -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            }
            else
            {
                Cursor.visible = true;
            }


            mousey = Mathf.Clamp(mousey, -45, 50);

            CharacterController controller = character.GetComponent<CharacterController>();

            Quaternion toApply = Quaternion.identity;
            if (Input.GetKey("space"))
            {
                vSpeed = climbSpeed;
            }
            else if (mousey <= 0)
            {
                vSpeed = 0;
            }
            else
            {
                vSpeed -= gravity * Time.deltaTime;
            }



            //Left mouse button allows for camera swivle while right mouse Button allows for character swivel
            if (Input.GetMouseButton(1))
            {
                lastView = Quaternion.AngleAxis(mousex, Vector3.up);
                lastView *= Quaternion.AngleAxis(mousey * (mousey < 0 ? .5f : 1.5f), Vector3.right);
            }
            if (Input.GetKey("space"))
            {
                //toApply *= Quaternion.AngleAxis(-20, character.right);
            }



            toApply *= lastView * Quaternion.AngleAxis(rollCurve.Evaluate(Mathf.Clamp(AngleDiff360(lastView.eulerAngles.y, character.rotation.eulerAngles.y), -90, 90)), Vector3.forward);


            character.rotation = Quaternion.Slerp(character.rotation, toApply, Time.deltaTime * rotationSpeed);
            Vector3 movement = character.forward.normalized * moveSpeed;

            if (vSpeed < 0)
            {
                vSpeed /= 4f;
            }
            movement.y += vSpeed;
            if (character.position.y >= maxHeight)
            {   
                movement.y = Mathf.Min(movement.y, 0);
            }

            
            controller.Move(movement * Time.deltaTime);

        }

    }
    
    public void KillCharacter()
    {
        dead = true;
    }

    void LateUpdate()
    {
        centerPoint.position = new Vector3(character.position.x, character.position.y + mouseYPosition, character.position.z);

        playerCamera.transform.localPosition = new Vector3(0, lift, zoom);
        playerCamera.LookAt(centerPoint);
        centerPoint.localRotation = Quaternion.Euler(mousey, mousex, 0);
    }

    public static float Rlerp(float start, float end, float query)
    {
        return Mathf.Clamp01((query-start)/(end-start));
    }

    public static float AngleDiff360(float start, float end)
    {
        float difference = end - start;

        if(difference < -180)
        {
            difference += 360;
        }
        if(difference > 180)
        {
            difference -= 360;
        }
        

        return difference;
    }
}
