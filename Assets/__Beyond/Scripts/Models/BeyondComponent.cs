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
        //TODO : maybe this can be in TemplateController instead (more robust)
        public static List<string> featureTags = new List<string>(){"InTerrain" , "SnapH" , "SnapV"};
        public Template template;
        //TODO : Use this to determine what the parent object should collide with, snap with, etc
        public BC_State state { get; protected set; }
        public List<Feature> features { get; protected set; }
        public Vector3 castBox { get; protected set; }
        public Vector3 pivotOffset { get; protected set; }
        public HashSet<GameObject> objectsTriggered { get; protected set; } // Objects this BO is colliding with
        public BeyondGroup beyondGroup;

        /// <summary>
        /// Whether or not an object tag means the object is based on a feature
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool tagIsFeatureTag(string t)
        {
            return featureTags.Contains(t);
        }

        public BeyondComponent()
        {
            state = BC_State.Ghost;
            beyondGroup = null;
            features = new List<Feature>();
            objectsTriggered = new HashSet<GameObject>();
        }

        public void initialise(Template t)
        {
            template = t;
            castBox = t.castBox;
            features = t.features;
            pivotOffset = t.pivotOffset;
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
            castBox = template.castBox;
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
                CreateGameObjectForFeature(f , i);
                i++;
            }
        }

        private void CreateGameObjectForFeature(Feature f , int i)
        {
            if (tagRequiresObject(f.tag))
            {
                //TO DO : all this coding depends on the feature's tag
                // If the tag requires a gameObject for this feature, create it here
                GameObject go = new GameObject();
                go.transform.parent = transform;
                go.transform.localPosition = f.offset;
                go.name = transform.gameObject.name + "_Feature_" + i;
                go.tag = f.tag;
                f.setGameObject(go);
                // Add a SphereCollider to the feature for snapping
                // If the tag requires a collider added to the gameObject's feature, create it here
                generateObjectCollider(f.tag, go);
            }
        }


        private bool tagRequiresObject(string tag)
        {
            // TODO move hardcoding to some kind of tag controller
            return tag == "InTerrain";
        }
        private void generateObjectCollider(string tag, GameObject go)
        {
            // TODO move hardcoding to some kind of tag controller
            if (tag=="InTerrain")
            {
                SphereCollider sc = go.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = 0.25f;
            }
        }


        public bool collidingWithBuilding(bool checkSameGroup = true)
        {
            foreach (GameObject g in objectsTriggered)
            {
                if (g.layer == LayerMask.NameToLayer("Buildings"))
                {
                    if (checkSameGroup || g.GetComponent<BeyondComponent>().beyondGroup != beyondGroup)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string objectsCollidingString()
        {
            return String.Join(",", objectsTriggered);
        }

        /// <summary>
        /// Returns all GameObject that are based on a feature (based on their tag, like "InTerrain" or "SnapH")
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameObject> collidingWithFeatureGameObjects()
        {
            return objectsTriggered.Where<GameObject>(g => tagIsFeatureTag(g.tag));
        }

        public List<Vector3Int> neighbourLinks(Feature f1, Feature f2)
        {
            List<Vector3Int> list1 = f1.canLinkTo;
            List<Vector3Int> list2 = f2.canLinkTo;
            return list1.Intersect<Vector3Int>(list2).ToList();
        }

        void OnTriggerEnter(Collider c)
        {
            objectsTriggered.Add(c.gameObject);
        }

        void OnTriggerExit(Collider c)
        {
            objectsTriggered.Remove(c.gameObject);
        }

    }
}