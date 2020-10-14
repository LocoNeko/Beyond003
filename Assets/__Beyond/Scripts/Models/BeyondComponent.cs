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

        public void initialise(List<Feature> lf, Vector3 cb)
        {
            features = lf;
            castBox = cb;

            //TODO : this is where we must go through all features and create GameObjects, colliders, textures, etc... based on the feature's type
            // THen we cna remove createAllFeatures
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
            //These are 4 features at the bottom of the BeyondComponent 
            Feature a1 = new Feature(new Vector3(0.5f, -0.5f, 0.5f), "InTerrain", "Terrain");
            a1.addLink(Vector3Int.right);
            a1.addLink(new Vector3Int(0, 0, 1));
            a1.addLink(Vector3Int.up);
            a1.addLink(Vector3Int.down);
            Feature a2 = new Feature(new Vector3(0.5f, -0.5f, -0.5f), "InTerrain", "Terrain");
            a2.addLink(Vector3Int.right);
            a2.addLink(new Vector3Int(0, 0, -1));
            a2.addLink(Vector3Int.up);
            a2.addLink(Vector3Int.down);
            Feature a3 = new Feature(new Vector3(-0.5f, -0.5f, -0.5f), "InTerrain", "Terrain");
            a3.addLink(Vector3Int.left);
            a3.addLink(new Vector3Int(0, 0, -1));
            a3.addLink(Vector3Int.up);
            a3.addLink(Vector3Int.down);
            Feature a4 = new Feature(new Vector3(-0.5f, -0.5f, 0.5f), "InTerrain", "Terrain");
            a4.addLink(Vector3Int.left);
            a4.addLink(new Vector3Int(0, 0, 1));
            a4.addLink(Vector3Int.up);
            a4.addLink(Vector3Int.down);
            features.Add(a1);
            features.Add(a2);
            features.Add(a3);
            features.Add(a4);
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