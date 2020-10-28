using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class TemplateController : MonoBehaviour
    {
        public static TemplateController Instance;
        public Dictionary<string,Template> templates { get; protected set; }

        void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("There should never be multiple Template controllers");
            }
            Instance = this;
            templates = new Dictionary<string, Template>();
            loadAllTemplates();
        }

        private void loadAllTemplates()
        {
            templates.Add("Foundation" , new Template(
                name: "Foundation" ,
                castBox: new Vector3(0.5f, 0.55f, 0.5f) ,
                prefab_go: Resources.Load<GameObject>("Prefabs/Foundation") ,
                pivotOffset: new Vector3(0, -0.95f, 0f) ,
                constraints: null
            ));
            templates.Add("Wall" , new Template(
                name: "Wall" ,
                castBox: new Vector3(0.5f, 1.45f, 0.1f) ,
                prefab_go: Resources.Load<GameObject>("Prefabs/Wall") ,
                pivotOffset: new Vector3(0f, 1.05f, -0.4f) ,
                cells: new List<Vector3Int>() {new Vector3Int(0,0,0), new Vector3Int(0,1,0), new Vector3Int(0,2,0)},
                constraints: null
            ));
            templates.Add("Wallhole" , new Template(
                name: "Wallhole" ,
                castBox: new Vector3(0.5f, 1.25f, 0.1f) ,
                prefab_go: Resources.Load<GameObject>("Prefabs/Wallhole") ,
                pivotOffset: new Vector3(0f, 1.25f, -0.4f) ,
                cells: new List<Vector3Int>() {new Vector3Int(0,0,0), new Vector3Int(0,1,0), new Vector3Int(0,2,0)},
                constraints: null
            ));
            templates.Add("Floor" , new Template(
                name: "Floor" ,
                castBox: new Vector3(0.5f, 0.05f, 0.5f) ,
                prefab_go: Resources.Load<GameObject>("Prefabs/Floor") ,
                pivotOffset: new Vector3(0, -0.45f, 0f) ,
                constraints: null
            ));
            //TODO - Roof
            //TODO - WallOpened
        }

        public static Material prefabMaterial(Template t)
        {
            return t.prefab.GetComponent<Renderer>().sharedMaterial;
        }

    }
}
