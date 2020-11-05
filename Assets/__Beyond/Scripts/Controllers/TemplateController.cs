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
                constraints: null ,
                dragDimensions: 2
            ));
            templates.Add("Wall" , new Template(
                name: "Wall" ,
                castBox: new Vector3(0.5f, 1.45f, 0.1f) ,
                prefab_go: Resources.Load<GameObject>("Prefabs/Wall") ,
                pivotOffset: new Vector3(0f, 1.05f, -0.4f) ,
                cells: new List<Vector3Int>() {new Vector3Int(0,0,0), new Vector3Int(0,1,0), new Vector3Int(0,2,0)},
                constraints: null ,
                dragDimensions: 1
            ));
            templates.Add("Wallhole" , new Template(
                name: "Wallhole" ,
                castBox: new Vector3(0.5f, 1.25f, 0.1f) ,
                prefab_go: Resources.Load<GameObject>("Prefabs/Wallhole") ,
                pivotOffset: new Vector3(0f, 1.25f, -0.4f) ,
                cells: new List<Vector3Int>() {new Vector3Int(0,0,0), new Vector3Int(0,1,0), new Vector3Int(0,2,0)},
                constraints: null ,
                dragDimensions: 1
            ));
            templates.Add("Floor" , new Template(
                name: "Floor" ,
                castBox: new Vector3(0.5f, 0.05f, 0.5f) ,
                prefab_go: Resources.Load<GameObject>("Prefabs/Floor") ,
                pivotOffset: new Vector3(0, -0.45f, 0f) ,
                constraints: null ,
                dragDimensions: 2
            ));
            //TODO - Roof
            //TODO - WallOpened
        }

        public static BeyondComponent CreateObject(string templateName)
        {
            Template template = TemplateController.Instance.templates[templateName];
            GameObject go = Instantiate(template.prefab);
            //TODO: Un-hardcode this shit
            go.layer = 0 ;
            //TODO : need to experiment with BoxColldier & trigger
            go.GetComponent<BoxCollider>().enabled = true;
            BeyondComponent bc = go.AddComponent<BeyondComponent>();
            bc.setTemplate(template);
            return bc ;
        }

        public static void PlaceObject(BeyondComponent bc , string name , BC_State state = BC_State.Blueprint)
        {
            bc.gameObject.name = name ;

            // if the object was not snapped to a group, create a new group
            if(bc.beyondGroup==null)
            {
                PlaceController.Instance.CreateNewBeyondGroup(bc);
            }
            // TODO : Really need to think hard about this: will the box collider as trigger really be a general case for all elements ?
            bc.gameObject.GetComponent<BoxCollider>().isTrigger = true;
            bc.gameObject.GetComponent<BoxCollider>().enabled = true;
            //TODO: Un-hardcode this shit
            bc.gameObject.layer = 9 ;
            // TODO clean this once dragging is beautiful
            if (state==BC_State.Blueprint)
            {
                bc.SetState(BC_State.Blueprint) ;
            }
            if (state==BC_State.Ghost)
            { // When dragging, we place ghosts rather than blueprints
                bc.SetState(BC_State.Ghost) ;
            }
        }

        public static Material prefabMaterial(Template t)
        {
            return t.prefab.GetComponent<Renderer>().sharedMaterial;
        }

    }
}
