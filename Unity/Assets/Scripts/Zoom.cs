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

    public Vector3 defaultPos;
    public Vector3 targetPos;

    [SerializeField] private Camera cam;
    public bool isTarget = false;
    // Start is called before the first frame update
    void Start()
    {
        zoom = cam.orthographicSize;
        defaultPos = cam.transform.position;
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

        if (Input.GetMouseButtonDown(1))
        {
            isTarget = !isTarget;

            if (isTarget)
            {
                Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                targetPos = new Vector3(p.x, p.y, defaultPos.z);
                cam.transform.position = targetPos;
                isTarget = false;
            }
        }
        if (Input.GetKey(KeyCode.B)) {
            cam.transform.position = defaultPos;
        }



        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        cam.orthographicSize = zoom;

    }

	private void OnMouseDown()
	{

	}
}
