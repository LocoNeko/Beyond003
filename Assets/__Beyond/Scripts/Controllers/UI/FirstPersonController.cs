using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Beyond
{
    public class FirstPersonController : MonoBehaviour
    {
        public static FirstPersonController Instance { get; protected set; }

        public CharacterController cc;
        public float speed = 12f;
        public float gravity = -9.8f;
        public float jumpHeight = 3f;
        float timer = 0f;
        bool running;

        public Transform groundCheck;
        float groundDistance = 0.4f;

        public Vector3 velocity;
        public bool isOnGround;

        void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There should never be multiple FirstPerson controllers");
            }
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            //if (UIController.Instance.gameMode == gameMode.free)
            //{
                timer += Time.deltaTime;
                if (timer > 0.2f)
                {
                    // TO DO : Should I check for different masks to prevent jumping from non-jumpable objects ?
                    // Does such a thing even exists ? 
                    isOnGround = Physics.CheckSphere(groundCheck.position, groundDistance, ConstraintController.getTerrainMask()) || Physics.CheckSphere(groundCheck.position, groundDistance, ConstraintController.getBuildingsMask());
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
                cc.Move(move * speed * (running ? 2.5f : 1) * Time.deltaTime);

                if (Input.GetButtonDown("Jump") && isOnGround)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                velocity.y += gravity * Time.deltaTime * 5f;
                cc.Move(velocity * Time.deltaTime);
            //}
        }

        public void Save(ref SavedGame game)
        {
            game.fp_position = transform.position ;
            game.fp_rotation = transform.rotation ;
        }

        public void Load(SavedGame game)
        {
            cc.enabled = false;
            cc.transform.position = game.fp_position ;
            cc.transform.rotation = game.fp_rotation ;
            cc.enabled = true;
        }

    }
}
