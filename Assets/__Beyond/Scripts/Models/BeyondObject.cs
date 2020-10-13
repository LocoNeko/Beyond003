using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Beyond
{
    // This script is linked to all objects that can be created & placed
    public class BeyondObject : MonoBehaviour
    {
        public List<Anchor> anchors { get; protected set; }
        public Vector3 castBox { get; protected set; }
        public HashSet<GameObject> objectsColliding { get; protected set; } // Objects this BO is colliding with
        public ObjectGroup objectGroup;
        public BeyondObject()
        {
            objectGroup = null;
            anchors = new List<Anchor>();
            objectsColliding = new HashSet<GameObject>();
        }

        public void setObjectGroup (ObjectGroup g)
        { // TODO should make that more robust : what if g is null ?
            unsetObjectGroup();
            objectGroup = g;
            g.addBeyondObject(this);
        }

        public void unsetObjectGroup()
        {
            if (objectGroup != null)
            {
                objectGroup.removeBeyondObject(this);
            }
            objectGroup = null;
        }

        public void createAllAnchors()
        {
            // TODO : clean this so the creation of a BeyondObject generates all needed anchors & castbox from its template
            castBox = new Vector3(5f, 0.1f, 5f);
            //These are 4 anchors at the bottom of the BeyondObject 
            Anchor a1 = new Anchor(new Vector3(0.5f, -0.5f, 0.5f), "InTerrain", "Terrain");
            a1.addLink(Vector3Int.right);
            a1.addLink(new Vector3Int(0, 0, 1));
            a1.addLink(Vector3Int.up);
            a1.addLink(Vector3Int.down);
            Anchor a2 = new Anchor(new Vector3(0.5f, -0.5f, -0.5f), "InTerrain", "Terrain");
            a2.addLink(Vector3Int.right);
            a2.addLink(new Vector3Int(0, 0, -1));
            a2.addLink(Vector3Int.up);
            a2.addLink(Vector3Int.down);
            Anchor a3 = new Anchor(new Vector3(-0.5f, -0.5f, -0.5f), "InTerrain", "Terrain");
            a3.addLink(Vector3Int.left);
            a3.addLink(new Vector3Int(0, 0, -1));
            a3.addLink(Vector3Int.up);
            a3.addLink(Vector3Int.down);
            Anchor a4 = new Anchor(new Vector3(-0.5f, -0.5f, 0.5f), "InTerrain", "Terrain");
            a4.addLink(Vector3Int.left);
            a4.addLink(new Vector3Int(0, 0, 1));
            a4.addLink(Vector3Int.up);
            a4.addLink(Vector3Int.down);
            anchors.Add(a1);
            anchors.Add(a2);
            anchors.Add(a3);
            anchors.Add(a4);
            int i = 0;
            foreach (Anchor a in anchors)
            {
                GameObject go = new GameObject();
                go.transform.parent = transform;
                go.transform.localPosition = a.offset;
                go.name = "Anchor_" + i++ + "_" + transform.gameObject.name;
                go.tag = a.tag;
                a.setGameObject(go); // For anchors that are represented by a gameObject, set it here
                // Add a SphereCollider to the anchors for snapping
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
                if (g.layer == PlaceController.Instance.buildingLayerMask && (checkSameGroup || (!checkSameGroup && g.GetComponent<BeyondObject>().objectGroup!=objectGroup)))
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

        public IEnumerable<GameObject> collidingWithAnchors()
        {
            return objectsColliding.Where<GameObject>(g => g.tag == "InTerrain");
        }

        void OnTriggerExit(Collider c)
        {
            objectsColliding.Remove(c.gameObject);
        }

        public List<Vector3Int> neighbourLinks(Anchor a1 , Anchor a2)
        {
            List<Vector3Int> list1 = a1.canLinkTo;
            List<Vector3Int> list2 = a2.canLinkTo;
            return list1.Intersect<Vector3Int>(list2).ToList();
        }
    }
}