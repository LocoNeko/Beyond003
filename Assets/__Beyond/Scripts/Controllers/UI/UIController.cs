using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public enum gameMode : int { free, build }
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; protected set; }
        public gameMode gameMode { get; protected set; }

        void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There should never be multiple UI controllers");
            }
            Instance = this;
            setGameMode(gameMode.free);
        }


        // Update is called once per frame
        void Update()
        {
            // TAB changes gameMode
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (gameMode == gameMode.free)
                {
                    setGameMode(gameMode.build);
                }
                else if (gameMode == gameMode.build)
                {
                    setGameMode(gameMode.free);
                }
            }
        }

        public void setGameMode(gameMode gm)
        { // Show the cursor only in interface mode
            gameMode = gm;
            if (gameMode == gameMode.free)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                // TODO : Show GUI
                Cursor.lockState = CursorLockMode.Confined;
            }

        }
    }
}

