using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class Anchor
    {
        public Vector3 offset { get; protected set;  } // Where is the anchor located relative to the object's centre
        public string tag;
        public List<string> snapToTags; // The tags of other anchors this anchor snaps to
        public List<GameObject> snappedTo;
        public GameObject gameObject { get; protected set; } // If this anchor is repsetned by a gameObject, record it here
        public List<Vector3Int> canLinkTo = new List<Vector3Int>(); // can this anchor be used to link to the neighbouring cell in one of the 6 basic directions ?

        public Anchor()
        {
            offset = Vector3.zero;
            tag = "";
            snapToTags= new List<string>();
            snappedTo = new List<GameObject>();
        }

        public Anchor(Vector3 v , string t , string snapToTag)
        {
            snapToTags = new List<string>();
            snappedTo = new List<GameObject>();
            offset = v;
            tag = t;
            addSnapToTag(snapToTag);
        }

        public void setGameObject(GameObject g)
        {
            gameObject = g;
        }

        public void addLink(Vector3Int v)
        {
            canLinkTo.Add(v);
        }

        public void addSnapToTag(string s)
        {
            if (!snapToTags.Contains(s))
            {
                snapToTags.Add(s);
            }
        }

        public void snapTo(GameObject go)
        {
            snappedTo.Add(go);
        }

        public void unsnapFrom(GameObject go)
        {
            if (snappedTo.Contains(go))
            {
                snappedTo.Remove(go);
            }
        }

    }
}