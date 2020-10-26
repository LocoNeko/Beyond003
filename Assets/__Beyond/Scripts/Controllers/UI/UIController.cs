using System;
using UnityEngine;
using TMPro;

namespace Beyond
{
    public enum gameMode : int { free, build }
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; protected set; }
        public gameMode gameMode { get; protected set; }
        TextMeshProUGUI TM_Infos ;
        TextMeshProUGUI TM_CurrentPosition ;
        public BeyondGroup closestGroup {get; protected set;}
        public Vector3Int positionInGroup ;
        public float heightOffset {get; protected set;}


        void Awake()
        {
            TM_CurrentPosition = GameObject.Find("Label_CurrentPosition").GetComponent<TextMeshProUGUI>();
            TM_Infos = GameObject.Find("Label_Infos").GetComponent<TextMeshProUGUI>() ;
            positionInGroup = new Vector3Int(-999,-999,-999);
        }

        void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There should never be multiple UI controllers");
            }
            Instance = this;
            setGameMode(gameMode.free);
        }

        public void SetHeighOffset(float h , float delta=0)
        {
            if (delta!=0)
            {
                heightOffset+=delta;

            }
            else
            {
                heightOffset = h ;
            }
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

            Place place = PlaceController.Instance.place ;

            // Shows the place name, hemisphere, and season, on 2 lines
            TM_Infos.text = String.Format("{0} ({1})\n{2}" , place.name , place.hemisphere , place.GetSeason()); 

            // Show First person position
            //TODO : no need if we are using third person camera
            Vector3 FPposition = FirstPersonController.Instance.transform.position ;
            TM_CurrentPosition.text = string.Format("X={0:0.00};Y={1:0.00};Z={2:0.00}\nClosest group: {3}, {4}" , FPposition.x ,FPposition.y , FPposition.z , (closestGroup==null ? "N/A" : closestGroup.name) , positionInGroup);
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

        public void SetClosestGroup(BeyondGroup g)
        {
            closestGroup = g ;
        }

    }
}

