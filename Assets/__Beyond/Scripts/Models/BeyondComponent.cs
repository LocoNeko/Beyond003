using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Beyond
{
    public enum BC_State : int { Ghost, Blueprint, Solid}

    // This script is attached to all objects specific to Beyond that can be created & placed
    public class BeyondComponent : MonoBehaviour
    {
        public Template template;
        //TODO : Use this to determine what it should collide with, snap with, etc
        public BC_State state { get; protected set; }
        public List<Feature> features { get; protected set; }
        public Vector3 castBox { get; protected set; }
        public HashSet<GameObject> objectsColliding { get; protected set; } // Objects this BO is colliding with
        public BeyondGroup beyondGroup;
        public BeyondComponent()
        {
            state = BC_State.Ghost;
            beyondGroup = null;
            features = new List<Feature>();
            objectsColliding = new HashSet<GameObject>();
        }

        public void initialise(Template t)
        {
            template = t;
            castBox = t.castBox;
            features = t.features;
        }

        public void setObjectGroup (BeyondGroup g)
        { // TODO should make that more robust : what if g is null ?
            unsetObjectGroup();
            beyondGroup = g;
            g.addBeyondComponent(this);
        }

        public void unsetObjectGroup()
        {
            if (beyondGroup != null)
            {
                beyondGroup.removeBeyondComponent(this);
            }
            beyondGroup = null;
        }

        // TODO : clean this so the creation of a BeyondComponent generates all needed features & castbox from its template
        public void createAllFeatures()
        {
            castBox = new Vector3(5f, 0.1f, 5f);
            foreach (Feature ft in template.features)
            { // Create the features based on the template's features. Can't do features = template.features as it make Unity create distinct instances of GameObjects below (took me 3 hours to figure this bug out)
                Feature f = new Feature(ft.offset, ft.tag, ft.snapToTags);
                foreach (Vector3Int v in ft.canLinkTo)
                {
                    f.addLink(v);
                }
                features.Add(f);
            }
            int i = 0;
            foreach (Feature f in features)
            {
                GameObject go = new GameObject();
                go.transform.parent = transform;
                go.transform.localPosition = f.offset;
                go.name = "Feature" + i++ + "_" + transform.gameObject.name;
                go.tag = f.tag;
                f.setGameObject(go); // For features that are represented by a gameObject, set it here
                // Add a SphereCollider to the feature for snapping
                SphereCollider sc = go.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = 2.5f;
            }
        }

        void OnTriggerEnter(Collider c)
        {
            //Debug.Log("OnTriggerEnter "+c.gameObject.name+", isTrigger="+c.isTrigger);
            objectsColliding.Add(c.gameObject);
        }


        public bool collidingWithBuilding(bool checkSameGroup=true)
        {
            foreach (GameObject g in objectsColliding)
            {
                // TODO : really ? looks spaghetti to me
                if (g.layer == PlaceController.Instance.buildingLayerMask && (checkSameGroup || (!checkSameGroup && g.GetComponent<BeyondComponent>().beyondGroup!=beyondGroup)))
                {
                    return true;
                }
            }
            return false;
        }

        public string objectsCollidingString()
        {
            return String.Join(",", objectsColliding);
        }

        public IEnumerable<GameObject> collidingWithFeatures()
        {
            return objectsColliding.Where<GameObject>(g => g.tag == "InTerrain");
        }

        void OnTriggerExit(Collider c)
        {
            objectsColliding.Remove(c.gameObject);
        }

        public List<Vector3Int> neighbourLinks(Feature f1 , Feature f2)
        {
            List<Vector3Int> list1 = f1.canLinkTo;
            List<Vector3Int> list2 = f2.canLinkTo;
            return list1.Intersect<Vector3Int>(list2).ToList();
        }
    }
}