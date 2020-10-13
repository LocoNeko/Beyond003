using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class FirstPersonController : MonoBehaviour
    {
        public CharacterController cc;
        public float speed = 12f;
        public float gravity = -9.8f;
        public float jumpHeight = 3f;
        float timer = 0f;
        bool running;

        public Transform groundCheck;
        float groundDistance = 0.4f;
        public LayerMask groundMask;
        public LayerMask buildingMask;

        public Vector3 velocity;
        public bool isOnGround;

        // Update is called once per frame
        void Update()
        {
            if (UIController.Instance.gameMode == gameMode.free)
            {
                timer += Time.deltaTime;
                if (timer > 0.2f)
                {
                    // TO DO : Should I check for different masks to prevent jumping from non-jumpable objects ?
                    // Does such a thing even exists ? 
                    isOnGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask) || Physics.CheckSphere(groundCheck.position, groundDistance, buildingMask);
                    timer = 0f;
                    if (isOnGround && velocity.y < 0)
                    {
                        velocity.y = -2f;
                    }
                }

                float x = Input.GetAxis("Horizontal");
                float z = Input.GetAxis("Vertical");
                running = Input.GetKey(KeyCode.LeftShift);

                Vector3 move = transform.right * x + transform.forward * z;
                cc.Move(move * speed * (running ? 3 : 1) * Time.deltaTime);

                if (Input.GetButtonDown("Jump") && isOnGround)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                velocity.y += gravity * Time.deltaTime * 5f;
                cc.Move(velocity * Time.deltaTime);
            }
        }
    }
}
