using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public Transform targetPosition { get; set; }
    public float maxHight { get; set; }
    public float minHight { get; set; }

    private float zoom;
    public float Zoom
    {
        get { return zoom; }

        set
        {
            if (maxHight < value) maxHight = value;
            zoom = value;
        }
    }

    void LateUpdate()
    {
        autoSetPositionCamera();
    }

    public void updateZoomCamera()
    {
        this.gameObject.GetComponent<Camera>().orthographicSize = maxHight + minHight - Zoom;
    }
    public void autoSetPositionCamera()
    {
        transform.position = targetPosition.position + Vector3.up * 10;
    }

}
