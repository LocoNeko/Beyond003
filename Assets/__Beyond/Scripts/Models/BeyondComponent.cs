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
        public Template template { get; protected set; }
        //TODO : Use this to determine what the parent object should collide with, snap with, etc
        public BC_State state { get; protected set; }
        public BeyondGroup beyondGroup { get; protected set; }
        
        // Where is this component inside this cell ?
        public cellSide side { get; protected set; }

        // Where is this component's object in its group, compared to the group's root position
        public Vector3Int groupPosition { get; protected set; }

        public List<Vector3Int> cells { get; protected set; }
        public HashSet<GameObject> objectsTriggered { get; protected set; } // Objects this BO is colliding with

        public BeyondComponent()
        {
            state = BC_State.Ghost;
            beyondGroup = null;
            cells = new List<Vector3Int>();
            objectsTriggered = new HashSet<GameObject>();
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
            /*
            string debug_string="insideTerrain overlap boxes=";
            foreach (Collider c in collidersHit)
            {
                debug_string += c.gameObject.name + ",";
            }
            Debug.Log(debug_string);
            */
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
            Debug.Log("OnTriggerEnter: "+c.gameObject.name);
            objectsTriggered.Add(c.gameObject);
        }

        void OnTriggerExit(Collider c)
        {
            objectsTriggered.Remove(c.gameObject);
        }


    }

}
