using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class ObjectGroup
    {
        public string name { get; protected set;}
        public List<BeyondObject> objectList { get; protected set; }
        public ObjectGroup(string s)
        {
            name = s;
            objectList = new List<BeyondObject>();
        }

        public bool addBeyondObject(BeyondObject bo)
        {
            if (objectList.Contains(bo))
            {
                return false;
            }
            else
            {
                objectList.Add(bo);
                return true;
            }
        }

        public bool removeBeyondObject(BeyondObject bo)
        {
            if (objectList.Contains(bo))
            {
                objectList.Remove(bo);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
