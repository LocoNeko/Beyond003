using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Beyond
{
    public enum BC_State : int { Ghost, Blueprint, Solid}

    public enum cellSide : int {Up , Down , Right , Left , Front , Back}

    // This script is attached to all objects specific to Beyond that can be created & placed
    public class BeyondComponent : MonoBehaviour
    {
        [field: SerializeField]
        public Template template { get; protected set; }
        //TODO : Use this to determine what the parent object should collide with, snap with, etc
        [field: SerializeField]
        public BC_State state { get; protected set; }
        [field: SerializeField]
        public BeyondGroup beyondGroup { get; protected set; }

        // Where is this component inside this cell ?
        [field: SerializeField]
        public cellSide side { get; protected set; }

        // Where is this component's object in its group, compared to the group's root position
        [field: SerializeField]
        public Vector3Int groupPosition { get; protected set; }

        [field: SerializeField]
        public List<Vector3Int> cells { get; protected set; }
        public HashSet<GameObject> objectsTriggered { get; protected set; } // Objects this BO is colliding with

        public BeyondComponent()
        {
            state = BC_State.Ghost;
            beyondGroup = null;
            cells = new List<Vector3Int>();
            objectsTriggered = new HashSet<GameObject>();
        }

        public void LoadComponent(SavedComponent sc)
        {
            template = TemplateController.Instance.templates[sc.template] ;
            state = sc.state ;
            beyondGroup = sc.group ;
            side = sc.side ;
            groupPosition = sc.groupPosition ;
            cells = sc.cells ;
        }

        public void setTemplate(Template t)
        {
            template = t;
        }

        public void SetState(BC_State newState)
        {
            state = newState ;
        }

        public void setObjectGroup (BeyondGroup g , Vector3Int p , cellSide cs , bool firstObject=false)
        { // TODO should make that more robust : what if g is null ?
            unsetObjectGroup();
            if (g!=null)
            {
                beyondGroup = g ;
                groupPosition = p ;
                side = cs ;
                // The first object should not be offset or rotated by the group, since it sets the group position and rotation 
                if (!firstObject)
                {
                    // Rotate the pivot offset by the group's rotation
                    Vector3 rotatedPivotOffset = template.pivotOffset ;
                    rotatedPivotOffset = g.rotation * rotatedPivotOffset ;
                    // Then  by the sideRotation
                    rotatedPivotOffset = sideRotation(cs) * rotatedPivotOffset ;

                    // Move the object to its template offset position
                    transform.position += rotatedPivotOffset;

                    // Rotate the object by the group's rotation + its cellSide rotation
                    transform.rotation = g.rotation * sideRotation(cs) ;
                }
                g.addBeyondComponent(this);
            }
        }

        //TODO : don't think this is used any more
        private static Quaternion sideRotation(cellSide cs)
        {
            switch(cs)
            {
                case cellSide.Right:
                    return Quaternion.Euler(0,90f,0);
                case cellSide.Front:
                    return Quaternion.Euler(0,180f,0);
                case cellSide.Left:
                    return Quaternion.Euler(0,270f,0);
            }
            return Quaternion.identity ;
        }

        public static cellSide closestSide(Vector3 distanceFromPivot)
        {
            float d = Vector3.Distance(distanceFromPivot , new Vector3(0,0,-0.5f));
            cellSide result = cellSide.Front ;
            float d2 = Vector3.Distance(distanceFromPivot , new Vector3(0,0,0.5f));
            if (d2 < d)
            {
                d = d2;
                result = cellSide.Back ;
            }
            d2 = Vector3.Distance(distanceFromPivot , new Vector3(-0.5f,0,0));
            if (d2 < d)
            {
                d = d2;
                result = cellSide.Left ;
            }
            d2 = Vector3.Distance(distanceFromPivot , new Vector3(0.5f,0,0));
            if (d2 < d)
            {
                d = d2;
                result = cellSide.Right ;
            }
            d2 = Vector3.Distance(distanceFromPivot , new Vector3(0,0.5f,0));
            if (d2 < d)
            {
                d = d2;
                result = cellSide.Up ;
            }
            d2 = Vector3.Distance(distanceFromPivot , new Vector3(0,-0.5f,0));
            if (d2 < d)
            {
                d = d2;
                result = cellSide.Down;
            }
            return result ;
        }

        public void unsetObjectGroup()
        {
            if (beyondGroup != null)
            {
                beyondGroup.removeBeyondComponent(this);
            }
            beyondGroup = null;
        }

        public bool insideTerrain()
        {
            Collider[] collidersHit = Physics.OverlapBox(transform.position , template.castBox , transform.rotation , ConstraintController.getTerrainMask()) ;
            // string debug_string="insideTerrain overlap boxes="; foreach (Collider c in collidersHit) debug_string += c.gameObject.name + ","; Debug.Log(debug_string);
            return (collidersHit.Length >0);
        }

        public bool collidingWithBuilding(bool checkSameGroup = true)
        {
            foreach (GameObject g in objectsTriggered)
            {
                //TODO : debug this. I need to make sure "triggers" are fine
                //Debug.Log("objectsTriggered= "+g.name);
                if (ConstraintController.layerIsInMask(g.layer, ConstraintController.getBuildingsMask()))
                {
                    if (!checkSameGroup || g.GetComponent<BeyondComponent>().beyondGroup != beyondGroup)
                    {
                        //Debug.Log("collidingWithBuilding TRUE. Object is in group " + g.GetComponent<BeyondComponent>().beyondGroup.name + " from group " + (beyondGroup!=null ? beyondGroup.name : "NULL"));
                        return true;
                    }
                }
            }
            //Debug.Log("collidingWithBuilding FALSE");
            return false;
        }

        void OnTriggerEnter(Collider c)
        {
            objectsTriggered.Add(c.gameObject); //Debug.Log("OnTriggerEnter: "+c.gameObject.name);
        }

        void OnTriggerExit(Collider c)
        {
            objectsTriggered.Remove(c.gameObject);
        }

        public int DragDimensions(out Vector3 p1 , out Vector3 p2)
        {
            // if we are on a line, the side will give us the axis
            // Line:  p1 is a normalised Vector in the axis of the line, p2 is the origin
            // Plane: p1 is a normal vector, p2 is a point in the plane
            if (template.dragDimensions==1)
            { // drag along a line
                // The line's axis must be rotated first by the group's rotation, second by the side's
                //TODO : Make sure that no side rotation corresponds to going right
                p1 = Vector3.Normalize(beyondGroup.rotation * sideRotation(side) * Vector3.right );
                p2 = transform.position - template.pivotOffset ;
                return 1 ;
            }
            if (template.dragDimensions==2)
            { // drag along a plane
                Vector3 forward = Vector3.Normalize(beyondGroup.rotation * sideRotation(side) * Vector3.forward) ;
                Vector3 right   = Vector3.Normalize(beyondGroup.rotation * sideRotation(side) * Vector3.right) ;
                p1 = Vector3.Cross(forward , right) ;
                p2 = transform.position - template.pivotOffset ;
                return 2 ;
            }
            // don't drag
            Debug.Log("DragDimensions returned 0: no dragging");
            p1 = Vector3.zero ;
            p2 = Vector3.zero ;
            return 0 ;
        }
    }

    /*
    * The SavedComponent serializable class will hold all information necessary to save and load BeyondComponents in their groups, and recreate gameObjects
    */

    [System.Serializable]
    public class SavedComponent
    {
        public string template ;
        public BC_State state ;
        public BeyondGroup group ;
        public cellSide side ;
        public Vector3Int groupPosition ;
        public List<Vector3Int> cells ;
        public Vector3 position ;
        public Quaternion rotation ;
        public string name ;
        public int layer ;
        public bool isTrigger ;
        public bool enabled ;
        public SavedComponent(BeyondComponent bc)
        {
            template = bc.template.name ;
            state = bc.state ;
            group = bc.beyondGroup ;
            side = bc.side ;
            groupPosition = bc.groupPosition ;
            cells = bc.cells ;
            position = bc.transform.position ;
            rotation = bc.transform.rotation ;
            name = bc.transform.gameObject.name ;
            layer = bc.transform.gameObject.layer ;
            isTrigger = bc.transform.gameObject.GetComponent<BoxCollider>().isTrigger ;
            enabled = bc.transform.gameObject.GetComponent<BoxCollider>().enabled ;
        }
    }

}
