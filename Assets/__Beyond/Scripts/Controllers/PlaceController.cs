using System;
using UnityEngine;
using TMPro;

namespace Beyond
{
    public class PlaceController : MonoBehaviour
    {
        public Place place {get; protected set;}
        // This is how many World unit there are in one "cell"
        public static readonly float cellSize = 1f;
        public static PlaceController Instance;

        void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There should never be multiple Place controllers");
            }
            Instance = this;
        }

        void Start()
        {
            place = new Place();
        }

        public void CreateNewBeyondGroup(BeyondComponent bc , string name=null)
        {
            if (name == null)
            { // Auto give name
                name = String.Format("Group {0:0000}",place.beyondGroups.Count);
            }
            // bc.transform.position - bc.template.pivotOffset : THIS IS ESSENTIAL
            // This allows us to properly set the pivot of the group 
            BeyondGroup group = new BeyondGroup(name , bc.transform.position - bc.template.pivotOffset , bc.transform.rotation);
            if (bc!=null)
            {
                group.addBeyondComponent(bc);
                // Vector3Int.zero because the first object in a group is at position [0,0,0]
                // TODO: cellSide.Down because the first object is always a foundation, but I shouldn't hardcode
                bc.setObjectGroup(group , Vector3Int.zero , cellSide.Down , true);
            }
            place.beyondGroups.Add(group);
        }


    }
}