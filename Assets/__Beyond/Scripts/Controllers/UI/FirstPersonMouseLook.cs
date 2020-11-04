using Beyond;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{

    public class FirstPersonMouseLook : MonoBehaviour
    {
        public static FirstPersonMouseLook Instance { get; protected set; }

        public float mouseSensitivity = 100f;
        float xRotation = 0f;

        void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There should never be multiple FirstPersonMouseLook controllers");
            }
            Instance = this;
        }

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

        public void Save(ref SavedGame game)
        {
            game.fplook_rotation = transform.rotation ;
        }

        public void Load(SavedGame game)
        {
            transform.rotation = game.fplook_rotation ;
        }

    }
}