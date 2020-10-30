using Beyond;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{

    public class FirstPersonMouseLook : MonoBehaviour
    {
        public float mouseSensitivity = 100f;
        float xRotation = 0f;


        // Update is called once per frame
        void Update()
        {
            if (UIController.Instance.gameMode == gameMode.free)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                transform.parent.Rotate(Vector3.up * mouseX);
            }
        }
    }
}