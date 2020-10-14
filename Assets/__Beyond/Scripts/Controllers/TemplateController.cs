using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Beyond
{
    public class TemplateController : MonoBehaviour
    {
        [SerializeField]
        private GameObject fundationPrefab;

        public static TemplateController Instance;
        public Dictionary<string,Template> templates { get; protected set; }

        void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There should never be multiple Template controllers");
            }
            Instance = this;
            loadAllTemplates();
        }

        private void loadAllTemplates()
        {
            templates = new Dictionary<string, Template>();
            // TODO : this is hardoced, but should be loaded from a json file
            // FUNDATION
            List<Feature> features = new List<Feature>();
            Feature f1 = new Feature(new Vector3(0.5f, -0.5f, 0.5f), "InTerrain", new List<string>{ "Terrain" });
            f1.addLink(Vector3Int.right);
            f1.addLink(new Vector3Int(0, 0, 1));
            f1.addLink(Vector3Int.up);
            f1.addLink(Vector3Int.down);
            Feature f2 = new Feature(new Vector3(0.5f, -0.5f, -0.5f), "InTerrain", new List<string> { "Terrain" });
            f2.addLink(Vector3Int.right);
            f2.addLink(new Vector3Int(0, 0, -1));
            f2.addLink(Vector3Int.up);
            f2.addLink(Vector3Int.down);
            Feature f3 = new Feature(new Vector3(-0.5f, -0.5f, -0.5f), "InTerrain", new List<string> { "Terrain" });
            f3.addLink(Vector3Int.left);
            f3.addLink(new Vector3Int(0, 0, -1));
            f3.addLink(Vector3Int.up);
            f3.addLink(Vector3Int.down);
            Feature f4 = new Feature(new Vector3(-0.5f, -0.5f, 0.5f), "InTerrain", new List<string> { "Terrain" });
            f4.addLink(Vector3Int.left);
            f4.addLink(new Vector3Int(0, 0, 1));
            f4.addLink(Vector3Int.up);
            f4.addLink(Vector3Int.down);
            features.Add(f1);
            features.Add(f2);
            features.Add(f3);
            features.Add(f4);
            Template fundation = new Template("Fundation", features, new Vector3(5f, 0.1f, 5f) , Resources.Load<GameObject>("Prefabs/Fundation"));
            //Debug.Log(String.Format("Template created with {0} features ",features.Count));
            templates.Add("Fundation", fundation);
            // TODO - WALL
            // TODO - FLATROOF
        }

        public GameObject TestCreate(string templateName)
        {
            GameObject go = Instantiate(templates[templateName].prefab);
            go.GetComponent<BoxCollider>().enabled = false;
            BeyondComponent bc = go.AddComponent<BeyondComponent>();
            bc.template = templates[templateName];
            bc.createAllFeatures();
            bc.unsetObjectGroup();
            return go;
        }


        public static Material prefabMaterial(Template t)
        {
            return t.prefab.GetComponent<Renderer>().sharedMaterial;
        }
    }
}
