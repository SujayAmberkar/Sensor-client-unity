using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

[System.Serializable]
public class MyData
{
    public float x;
    public float y;
    public float z;
}

public class WebsocketClient : MonoBehaviour
{
    WebSocket wss;
    private float rotationSpeed = 30.0f;  // Adjust the speed based on your preference

    private Vector3 cumulativeRotation = Vector3.zero;

    // Start is called before the first frame update
    async void Start()
        {

        wss = new WebSocket("ws://localhost:8080");

        wss.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };
        wss.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        wss.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        wss.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received message from server: " + message);

            // Pass the message to a method for further processing in Unity
            if (message!=null)
            {
                HandleServerMessage(message);
            }
            
        };


        await wss.Connect();
    }

    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
            wss.DispatchMessageQueue();
    #endif
    }


    private async void OnApplicationQuit()
    {
        await wss.Close();
    }

    // Method to handle the incoming message from the server
    void HandleServerMessage(string message)
    {
        // Deserialize the JSON string into MyData object
        MyData data = JsonUtility.FromJson<MyData>(message);

        // Access the gyroscopic rates
        float xRate = data.x;
        float yRate = data.y;
        float zRate = data.z;

        // Convert gyro rates to rotation angles (using time.deltaTime for time-based integration)
        float xAngle = xRate * Time.deltaTime * rotationSpeed  * Mathf.Rad2Deg;
        float yAngle = yRate * Time.deltaTime * rotationSpeed  * Mathf.Rad2Deg;
        float zAngle = zRate * Time.deltaTime * rotationSpeed * Mathf.Rad2Deg;

        // Update the cumulative rotation
        cumulativeRotation += new Vector3(xAngle, yAngle, zAngle);

        // Set the rotation of your GameObject
        transform.rotation = Quaternion.Euler(cumulativeRotation);
    }
}
