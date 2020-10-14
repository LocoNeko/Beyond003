using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    /* TODO : This is a bad name as this could be points in space, colliders, textures, even sound...
     * Call this :
     * - SubComponent ? awful...
     * - Feature
     * - Trait
     * - ComponentFeature
     */
    public class Feature
    {
        public Vector3 offset { get; protected set;  } // Where is the feature located relative to the object's centre
        public string tag;
        public List<string> snapToTags; // The tags of other features this feature snaps to
        public List<GameObject> snappedTo;
        public GameObject gameObject { get; protected set; } // If this feature is represented by a gameObject, record it here
        public List<Vector3Int> canLinkTo = new List<Vector3Int>(); // can this feature be used to link to the neighbouring cell in one of the 6 basic directions ?

        public Feature()
        {
            offset = Vector3.zero;
            tag = "";
            snapToTags= new List<string>();
            snappedTo = new List<GameObject>();
        }

        public Feature(Vector3 v , string t , string snapToTag)
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