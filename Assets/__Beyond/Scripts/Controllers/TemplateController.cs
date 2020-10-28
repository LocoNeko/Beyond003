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


        public static void CreateObject(string templateName , ref GameObject go , ref BeyondComponent bc)
        {
            Template template = TemplateController.Instance.templates[templateName];
            go = Instantiate(template.prefab);
            //TODO: Un-hardcode this shit
            go.layer = 0 ;
            //TODO : need to experiment with BoxColldier & trigger
            go.GetComponent<BoxCollider>().enabled = true;
            bc = go.AddComponent<BeyondComponent>();
            bc.setTemplate(template);
        }

        public static void PlaceObject(ref GameObject go , string name)
        {
            BeyondComponent bc = go.GetComponent<BeyondComponent>() ;
            // Set the material back to the prefab's material to get rid of the green or red colours
            go.GetComponent<Renderer>().material = prefabMaterial(bc.template);
            // TODO : Really need to think hard about this: will the box collider as trigger really be a general case for all elements ?
            go.GetComponent<BoxCollider>().isTrigger = false;
            go.GetComponent<BoxCollider>().enabled = true;
            //TODO: Un-hardcode this shit
            go.layer = 9 ;
            bc.SetState(BC_State.Blueprint) ;
            go.name = name ;
            // if the object was not snapped to a group, create a new group
            if(bc.beyondGroup==null)
            {
                PlaceController.Instance.CreateNewBeyondGroup(bc);
            }
            go = null;
            //TODO : If SHIFT is pressed, allow queuing of objects to be placed
        }

        public static Material prefabMaterial(Template t)
        {
            return t.prefab.GetComponent<Renderer>().sharedMaterial;
        }

    }
}
