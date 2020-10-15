using System;
using UnityEngine;

namespace Beyond
{
    public class PlaceController : MonoBehaviour
    {
        Place place;
        // This is how many World unit there are in one "cell"
        public static readonly float cellSize = 1f;
        public static PlaceController Instance;
        public LayerMask buildingLayerMask;
        public LayerMask terrainLayerMask;

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


        public void CreateNewBeyondGroup(BeyondComponent bc=null , string name=null)
        {
            if (name == null)
            { // Auto give name
                name = String.Format("Group {0:0000}",place.beyondGroups.Count);
            }
            BeyondGroup group = new BeyondGroup(name);
            if (bc!=null)
            {
                group.addBeyondComponent(bc);
                bc.setObjectGroup(group);
            }
            place.beyondGroups.Add(group);
        }

        /// <summary>
        /// For a given object and a direction, find where the centre of the neighbouring cell would be
        /// </summary>
        /// <param name="go"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 neighbourCentre(GameObject go, Vector3Int direction)
        {
            GameObject g = new GameObject();
            g.transform.position = go.transform.position + cellSize * (Vector3)direction;
            g.transform.RotateAround(go.transform.position, go.transform.right, go.transform.rotation.eulerAngles.x);
            g.transform.RotateAround(go.transform.position, go.transform.up, go.transform.rotation.eulerAngles.y);
            g.transform.RotateAround(go.transform.position, go.transform.forward, go.transform.rotation.eulerAngles.z);
            Vector3 result = g.transform.position;
            Destroy(g);
            return result;
        }

    }
}