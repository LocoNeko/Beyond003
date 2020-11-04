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

        public void Load(SavedGame game)
        {
            place = game.place ;
            // I need to initiate the componentList of the group. It doesn't exist as it can't be serialised
            foreach (SavedComponent data in game.components)
            {
                GameObject go = Instantiate(TemplateController.Instance.templates[data.template].prefab) ;
                BeyondComponent bc = go.AddComponent<BeyondComponent>() ;
                bc.LoadComponent(data) ;
                go.SetActive(false) ;
                go.transform.position = data.position ;
                go.transform.rotation = data.rotation ;
                go.name = data.name ;
                go.layer = data.layer ;
                go.GetComponent<BoxCollider>().isTrigger = data.isTrigger ;
                go.GetComponent<BoxCollider>().enabled = data.enabled ;
                go.SetActive(true);
                place.beyondGroups.Find(group => group == data.group).addBeyondComponent(bc) ;
            }
        }

        public void CreateNewBeyondGroup(BeyondComponent bc , string name=null)
        {
            if (name == null)
            { // Auto give name
                name = String.Format("Group {0:0000}",place.beyondGroups.Count);
            }
            // bc.transform.position - bc.template.pivotOffset : THIS IS ESSENTIAL - This allows us to properly set the pivot of the group 
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