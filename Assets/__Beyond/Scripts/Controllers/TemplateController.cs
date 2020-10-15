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
            Template fundation = new Template("Fundation", features, new Vector3(0.5f, 0.1f, 0.5f) , Resources.Load<GameObject>("Prefabs/Fundation"));
            templates.Add("Fundation", fundation);
            // WALL
            List<Feature> features2 = new List<Feature>();
            Feature f21 = new Feature(new Vector3(0f, -0.5f, -0.5f), "SnapH", new List<string> { "Snap" });
            f1.addLink(Vector3Int.right);
            f1.addLink(new Vector3Int(0, 0, 1));
            f1.addLink(Vector3Int.up);
            f1.addLink(Vector3Int.down);
            Feature f22 = new Feature(new Vector3(0.5f, 0f, -0.5f), "SnapV", new List<string> { "Snap" });
            f2.addLink(Vector3Int.right);
            f2.addLink(new Vector3Int(0, 0, -1));
            f2.addLink(Vector3Int.up);
            f2.addLink(Vector3Int.down);
            Feature f23 = new Feature(new Vector3(0f, 2f, -0.5f), "SnapH", new List<string> { "Snap" });
            f3.addLink(Vector3Int.left);
            f3.addLink(new Vector3Int(0, 0, -1));
            f3.addLink(Vector3Int.up);
            f3.addLink(Vector3Int.down);
            Feature f24 = new Feature(new Vector3(-0.5f, 0f, -0.5f), "SnapV", new List<string> { "Snap" });
            f4.addLink(Vector3Int.left);
            f4.addLink(new Vector3Int(0, 0, 1));
            f4.addLink(Vector3Int.up);
            f4.addLink(Vector3Int.down);
            features2.Add(f21);
            features2.Add(f22);
            features2.Add(f23);
            features2.Add(f24);
            Template wall = new Template("Wall", features2, new Vector3(0.5f, 0.1f, 0.5f), Resources.Load<GameObject>("Prefabs/Wall"));
            templates.Add("Wall", wall);
            // TODO - WALL
            // TODO - FLATROOF
        }

        public GameObject CreateGameObject(string templateName)
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
