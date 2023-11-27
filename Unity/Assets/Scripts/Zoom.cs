 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    public float zoom;
    public float zoomMultiplier = 4f;
    public float minZoom = 2f;
    public float maxZoom = 8f;
    public float velocity = 0f;
    public float smoothTime = 0.25f;

    [SerializeField] private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        zoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.O))
        {
            zoom += 0.5f * zoomMultiplier*Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.P)) {
            zoom -= 0.5f * zoomMultiplier*Time.deltaTime;
        }

        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        cam.orthographicSize = zoom;
    }
}
