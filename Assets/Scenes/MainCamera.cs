using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class MainCamera : MonoBehaviour
{
    public Camera mainCamera;
    public Transform cameraTransform;
    private Transformer originalTransform;
    private Vector3 direction;
    private Vector3 positionChange;
    private float deltaTheta = 0.5f;
    private float deltaZ = 0.01f;
    private bool isDebug = false;
    private MyTcpListener server;
    private ImageCapture imageCapture;
    private String collisionData;       // CollisionInfo, stored in a String.

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = gameObject.GetComponent(typeof(Camera)) as Camera;
        originalTransform = new Transformer(mainCamera.transform);  // Make a copy, not a reference.
        cameraTransform = mainCamera.transform;
        positionChange = new Vector3(0f, 0f, 0f);
        direction = new Vector3(0f,0f,0f);
        imageCapture = gameObject.AddComponent<ImageCapture>() as ImageCapture;
        server = new MyTcpListener(this);
        server.startServer();
        imageCapture.setServer(server);
        clearCollisions();
    }
    void restart()
    {
        originalTransform.copyTo(cameraTransform);      // a reference to the actual camera.transform...
        positionChange = new Vector3(0f, 0f, 0f);
        direction = new Vector3(0f,0f,0f);
        clearCollisions();
    }
    // Use the keyboard keys shown below to move the camera through the scene.
    String getKeyboardRequest()
    {
        String request="";
        if ( Input.GetKey("l") )
            request = "left";
        if ( Input.GetKey("r"))
            request = "right";
        if ( Input.GetKey("u"))
            request = "up";
        if ( Input.GetKey("d"))
            request = "down";
        if ( Input.GetKey( "f"))
            request = "forward";
        if ( Input.GetKey("b"))
            request = "backward";
        if ( Input.GetKey("s"))
            request = "start";      // restart from initial position.
        return request;
    }
    // Update is called once per frame
    void Update()
    {
        String request = server.getReceivedMessage();
        if ( request == "image")
        {
            // The client asks for the current screen capture.
            //Debug.Log("About to Capture Image!");
            imageCapture.startCapture();
            request = null;
        }
        else if ( request == "collisions")
        {
            // The client asks for the current screen capture.
            //Debug.Log("About to Capture Image!");
            server.setMessageToSend(collisionData);
            request = null;
        }
        else if (request != null )
        {
            //Debug.Log("From Update(), request is " + request);
            server.setMessageToSend(request.ToUpper());
            // We're going to move, so ensure stale collision String is cleared.
            clearCollisions();
        }
        else
        {
            request = getKeyboardRequest();
        }
        positionChange = new Vector3(0f, 0f, 0f);
        // The direction is really the axis around which we rotate.
        // Eg., looking "up" means rotate around our X axis.
        switch (request)
        {
            case "left":
                direction.y -= deltaTheta;
                break;
            case "right":
                direction.y += deltaTheta;
                break;
            case "up":
                direction.x -= deltaTheta;
                break;
            case "down":
                direction.x += deltaTheta;
                break;
            case "forward":
                positionChange.z = deltaZ;
                break;
            case "backward":
                positionChange.z = -deltaZ;
                break;
            case "start":
                restart();
                break;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        float sweep = 2f;   // For a moderately fast moving camera.
        cameraTransform.Translate(positionChange, Space.Self);
        Vector3 temp = cameraTransform.position;
        temp.y = originalTransform.position.y;
        cameraTransform.position = temp;       // A hack to keep robot on ground.
        if ( isDebug && (positionChange.z != 0f) )
            Debug.Log("temp="+temp);
        // Use this for direction.
        Quaternion change = Quaternion.Euler(direction.x, direction.y, direction.z);
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, change, Time.deltaTime * sweep);
    }
    //===============================================
    // Clear collision data, typically because we're about to make a
    // move, and have ignored prior move's collision results (if any).
    private void clearCollisions()
    {
        collisionData = "COLLISIONS: 0";
    }
    private string collisionMessage(string title, Collision collisionInfo)
    {
        StringBuilder sb = new StringBuilder(50);
        sb.AppendLine(title + ", " + collisionInfo.gameObject.ToString() + ", " + collisionInfo.contacts.Length);
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            Vector3 localPoint = cameraTransform.InverseTransformPoint(contact.point);
            Debug.LogFormat("world{0}, local{1}", contact.point, localPoint);
            sb.AppendLine(localPoint.ToString());
        }
        return sb.ToString();
    }
    private void OnCollisionEnter(Collision collisionInfo)
    {
        Debug.LogFormat("Collision with: {0}, num={1}", collisionInfo.gameObject, collisionInfo.contacts.Length );
        collisionData = collisionMessage("COLLISIONS: ", collisionInfo);
    }
    private void OnCollisionStay(Collision collisionInfo)
    {
        Debug.LogFormat("Contact with: {0}, num={1}", collisionInfo.gameObject, collisionInfo.contacts.Length );
        collisionData = collisionMessage("CONTACTS: ", collisionInfo);
    }
    private void OnCollisionExit(Collision collisionInfo)
    {
        Debug.LogFormat("Exit with: {0}, num={1}", collisionInfo.gameObject,  collisionInfo.contacts.Length);
        collisionData = collisionMessage("EXITS: ", collisionInfo);
    }
}
