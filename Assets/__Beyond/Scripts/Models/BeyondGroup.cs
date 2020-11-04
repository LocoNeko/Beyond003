using System.Collections.Generic;
using UnityEngine;
using System;

namespace Beyond
{
    [System.Serializable]

    public class BeyondGroup
    {
        public Vector3 position { get; protected set;}
        public Quaternion rotation { get; protected set;}
        public Vector3 rightNormalised { get; protected set;}
        public Vector3 forwardNormalised { get; protected set;}
        public Vector3 upNormalised { get; protected set;}
        public string name { get; protected set;}
        [field: NonSerialized]
        public List<BeyondComponent> componentList { get; protected set; }
        [field:NonSerialized]
        public GameObject groupObject { get; protected set; }
        public BeyondGroup(string s , Vector3 p , Quaternion r)
        {
            position = p;
            rotation = r ;
            name = s;
            componentList = new List<BeyondComponent>();
            rightNormalised = ElementPlacementController.RotateAroundPoint(Vector3.right , Vector3.zero , r) ;
            forwardNormalised = ElementPlacementController.RotateAroundPoint(Vector3.forward , Vector3.zero , r) ;
            upNormalised = ElementPlacementController.RotateAroundPoint(Vector3.up , Vector3.zero , r) ;
            CreateGroupObject();
        }

        public void CreateGroupObject()
        {
            groupObject = new GameObject() ;
            groupObject.name = name ;
        }

        public bool addBeyondComponent(BeyondComponent bc)
        {
            // On-demand creation of componentList (for deserialization)
            if (componentList == null) componentList = new List<BeyondComponent>();

            if (componentList.Contains(bc)) return false;
            else
            {
                componentList.Add(bc);
                // On-demand creation of group GameObject (for deserialization)
                if (groupObject==null) CreateGroupObject();
                
                bc.transform.SetParent(groupObject.transform , false) ;
                return true;
            }
        }

        public bool removeBeyondComponent(BeyondComponent bc)
        {
            if (componentList.Contains(bc))
            {
                componentList.Remove(bc);
                return true;
            }
            return false;
        }

        public List<BeyondComponent> BeyondComponentsAt(Vector3Int p)
        {
            return componentList.FindAll(bc => bc.groupPosition == p) ;
        }

    }
}
