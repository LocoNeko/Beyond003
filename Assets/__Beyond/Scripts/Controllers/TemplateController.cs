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
            // TODO : this is hardoced, but should be loaded from file
            // FUNDATION
            List<Feature> features = new List<Feature>();
            Feature f1 = new Feature(new Vector3(0.5f, -0.5f, 0.5f), "InTerrain", "Terrain");
            f1.addLink(Vector3Int.right);
            f1.addLink(new Vector3Int(0, 0, 1));
            f1.addLink(Vector3Int.up);
            f1.addLink(Vector3Int.down);
            Feature f2 = new Feature(new Vector3(0.5f, -0.5f, -0.5f), "InTerrain", "Terrain");
            f2.addLink(Vector3Int.right);
            f2.addLink(new Vector3Int(0, 0, -1));
            f2.addLink(Vector3Int.up);
            f2.addLink(Vector3Int.down);
            Feature f3 = new Feature(new Vector3(-0.5f, -0.5f, -0.5f), "InTerrain", "Terrain");
            f3.addLink(Vector3Int.left);
            f3.addLink(new Vector3Int(0, 0, -1));
            f3.addLink(Vector3Int.up);
            f3.addLink(Vector3Int.down);
            Feature f4 = new Feature(new Vector3(-0.5f, -0.5f, 0.5f), "InTerrain", "Terrain");
            f4.addLink(Vector3Int.left);
            f4.addLink(new Vector3Int(0, 0, 1));
            f4.addLink(Vector3Int.up);
            f4.addLink(Vector3Int.down);
            features.Add(f1);
            features.Add(f2);
            features.Add(f3);
            features.Add(f4);
            Template fundation = new Template("Fundation", features, new Vector3(5f, 0.1f, 5f) , Resources.Load<GameObject>("Prefabs/Fundation"));
            templates.Add("Fundation", fundation);
            // TODO - WALL
            // TODO - FLATROOF
        }

        public GameObject createFromTemplate(Template template )
        {
            GameObject go = Instantiate(template.prefab);
            go.name = template.name;
            BeyondComponent bc = go.AddComponent<BeyondComponent>();
            bc.initialise(template.features, template.castBox);
            // TODO : this will actually replace the createAllFeatures() and we can be on our merry way
            return go;
        }

    }
}
