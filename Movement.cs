using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Vector3 rotation = Vector3.zero;
    [SerializeField] float speed = 2000;
    [SerializeField] Vector2 sensitivity = new Vector2(10, 10);

    // Update is called once per frame
    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = System.Convert.ToInt32(Input.GetKey(KeyCode.E)) - System.Convert.ToInt32(Input.GetKey(KeyCode.Q));

        float r_x = Input.GetAxis("Mouse X");
        float r_y = -Input.GetAxis("Mouse Y");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.Translate(x * speed, y * speed, z * speed);
        }
        else
        {
            transform.Translate(x, y, z);
        }

        rotation.y += Input.GetAxis("Mouse X") * sensitivity.x;
        rotation.x += -Input.GetAxis("Mouse Y") *sensitivity.y;
        transform.eulerAngles = (Vector2)rotation;
    }
}
