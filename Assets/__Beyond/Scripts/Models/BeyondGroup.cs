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
    }
}
