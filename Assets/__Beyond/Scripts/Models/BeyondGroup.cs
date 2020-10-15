using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class BeyondGroup
    {
        public string name { get; protected set;}
        public List<BeyondComponent> componentList { get; protected set; }
        public BeyondGroup(string s)
        {
            name = s;
            componentList = new List<BeyondComponent>();
        }

        public bool addBeyondComponent(BeyondComponent bc)
        {
            if (componentList.Contains(bc))
            {
                return false;
            }
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
            else
            {
                return false;
            }
        }

        public bool hasBeyondComponentAtCoordinate(Vector3 p , float tolerance=0.01f)
        {
            foreach(BeyondComponent bc in componentList)
            {
                //TODO : I should not need to do that, but got a bug about trying to access a destroyed bc when snapping
                if (bc!=null)
                {
                    //This is the position of the GameObject this BeyondComponent is attached to, corrected by the offset (if any)
                    Vector3 bcp = bc.transform.gameObject.transform.position - bc.pivotOffset;
                    if (Vector3.Distance(p, bcp) < tolerance)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
