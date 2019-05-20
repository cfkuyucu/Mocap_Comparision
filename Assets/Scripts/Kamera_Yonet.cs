using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kamera_Yonet : MonoBehaviour
{
    private float x;
    private float y;
    private Vector3 rotateValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        y = Input.GetAxis("Mouse X");
        x = Input.GetAxis("Mouse Y");
        //Debug.Log(x + ":" + y);
        rotateValue = new Vector3(3 * x, y * -3, 0);
        transform.eulerAngles -= rotateValue;
    }
}
