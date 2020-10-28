using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    [System.Serializable]
    public class BeyondGroup
    {
        public Vector3 position { get; protected set;}
        public Quaternion rotation { get; protected set;}
        public string name { get; protected set;}
        public List<BeyondComponent> componentList { get; protected set; }
        public BeyondGroup(string s , Vector3 p , Quaternion r)
        {
            position = p;
            rotation = r ;
            name = s;
            componentList = new List<BeyondComponent>();
        }

        public bool addBeyondComponent(BeyondComponent bc)
        {
            if (componentList.Contains(bc)) return false;
            else
            {
                componentList.Add(bc);
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
