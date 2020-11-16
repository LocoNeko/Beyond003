using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{

    public class ConstraintController : MonoBehaviour
    {
        public static float FoundationInTerrainBy = 0.2f;

        public static LayerMask getMask(string s)
        {
            //TODO : make this a bit better with actual masks of several layers if needed
            return LayerMask.GetMask(s);
        }

        // Convenience functions
        public static LayerMask getTerrainMask()
        {
            return getMask("Ground");
        }

        public static LayerMask getTreesMask()
        {
            return getMask("Trees");
        }

        public static LayerMask getTerrainAndTreesMask()
        {
            return LayerMask.GetMask(new string[] { "Ground", "Trees" });
        }
        public static LayerMask getBuildingsMask()
        {
            return getMask("Buildings");
        }

        public static bool layerIsInMask(int l , LayerMask lm)
        {
            int layerMask = 1 << l;
            return (layerMask == lm.value);
        }


        // TODO : should this really be hardcoded this badly ? I might not need to even put that in the templates but just have a list somewhere
        // OR: not even needed since all elemetns should be more or less above terrain ?
        /*
        public static bool ShowOnTerrain(Template t)
        {
            return t.name == "Foundation" ;
        }
        */

        //IMPORTANT : go.transform.position cannot be used since we're trying to place the GameObject through this method
        //rotation is fine (even if we just created the go, rotation will just be Quaternion.Identity)
        public static Vector3 PlaceGhost(BeyondComponent bc , Vector3 onPoint , LayerMask layerMask)
        {
            if (bc==null) return onPoint;

            // As a rule, the result is the same as the pointOnTerrain, we are just applying some filter below
            Vector3 result = onPoint ;
            
            result = GetPointOnLayer(bc , onPoint , layerMask) ;
            return result;
        }

        // given a point, get an object's postion above or below it so that the object is exactly on the Terrain
        public static Vector3 GetPointOnLayer(BeyondComponent bc , Vector3 point , LayerMask layerMask)
        {
            if (bc==null) return point;

            Vector3 result = point ;
            result.y = PlaceController.Instance.place.Height ; // Cast from the highest possible altitude
            RaycastHit hitInfo;
            Physics.BoxCast(result, bc.template.castBox, Vector3.down, out hitInfo, bc.transform.rotation , Mathf.Infinity, layerMask); //TODO - is this better ? : Physics.BoxCast(point, bc.template.castBox, Vector3.down, out hitInfo, go.transform.rotation);
            // Half the height of the object is bc.template.castBox.y
            //TODO : am I sure of that ? We need an offset for Foundations, by how much they can be insde terrain
            result.y = hitInfo.point.y + bc.template.castBox.y ;
            if (bc.template.name == "Foundation")
                result.y += FoundationInTerrainBy - bc.template.castBox.y * 2; //FoundationInTerrainBy
            return result ;
        }

        //TODO : All this is currently hardcoded but should just do CheckConstraints(bc)
        public static bool CanPlace(BeyondComponent bc)
        {
            // Won't work on a NULL BeyondComponent
            if (bc==null)
                return false;

            // can't already have a the same object at the same place & position
            if (IsTemplatePresentHere(bc.beyondGroup, bc.groupPosition, bc.template.name, bc.side)) 
                return false;

            // Object can't collide with other objects in different group 
            if (bc.collidingWithBuilding())
                return false ;

            if (bc.template.name=="Foundation")
            {
                // 1 - Foundations must be partially inside terrain, but their top must not be covered by it
                if (!BaseInTerrain(bc)) return false;
                if (!TopClear(bc , getTreesMask())) return false;
                if (AllClear(bc, ConstraintController.getTreesMask())) return true;
            }
            else
            {
                // 2 -All non-foundations objects must be clear of terrain and trees- return false immediately if they're not
                if(AllClear(bc , getTerrainAndTreesMask())) return true ;
            }

            //3 - All non-foundations must be snapped to another building part
            if (bc.template.name!="Foundation" && bc.beyondGroup==null)
            {
                return false ;
            }

            return false;
            //5 - Think of moveable objects later (they're not in groups, so they don't snap = constraints are easier)
        }

        public static bool CanSnapTo(BeyondComponent bc , BeyondGroup group , Vector3Int here)
        {
            if (group==null)
                return false ;

            if (bc.template.name=="Foundation")
            {
                // must not have the same object (Foundation) here
                if (IsTemplatePresentHere(group , here, "Foundation")) return false ;
                // Must have a neighbouring Foundation in X or Z, same Y
                if (IsTemplatePresentHere(group , new Vector3Int(here.x-1 , here.y , here.z), "Foundation")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x+1 , here.y , here.z), "Foundation")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y , here.z-1), "Foundation")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y , here.z+1), "Foundation")) return true ;
                Debug.Log("No neighbouring foundation  in group "+group.name);
                return false;
            }
            if (bc.template.name=="Wall")
            {
                if (IsTemplatePresentHere(group, here, bc.template.name, BeyondComponent.getSideByRotation(bc.transform.rotation))) return false; // Can't have wall in the same position and side

                // There must be a foundation to place a Wall
                if (IsTemplatePresentHere(group , here, "Foundation")) 
                    return true ; 
                return false;
            }
            if (bc.template.name=="Wallhole")
            {
                if (IsTemplatePresentHere(group, here, bc.template.name, BeyondComponent.getSideByRotation(bc.transform.rotation))) return false; // Can't have wallhole in the same position and side

                // There must be a foundation to place a Wall
                if (IsTemplatePresentHere(group , here, "Foundation")) 
                {
                    return true ; 
                }
                return false ;
            }
            if (bc.template.name=="Floor")
            {
                // must not have the same object (Floor) here
                if (IsTemplatePresentHere(group , here, "Floor")) return false ;

                // There must be a Wall 3 cells below
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y-3 , here.z), "Wall")) 
                {
                    return true ; 
                }
                // Or a floor next to here
                if (IsTemplatePresentHere(group , new Vector3Int(here.x-1 , here.y , here.z), "Floor")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x+1 , here.y , here.z), "Floor")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y , here.z-1), "Floor")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y , here.z+1), "Floor")) return true ;
                return false;
            }
            return false ;
        }

        public static bool IsTemplatePresentHere(BeyondGroup group , Vector3Int here , string t_name)
        {
            if (group == null) return false;
            return group.BeyondComponentsAt(here).Exists(bc => bc.template.name == t_name && bc.state!=BC_State.Ghost) ;
        }

        public static bool IsTemplatePresentHere(BeyondGroup group , Vector3Int here , string t_name , cellSide cs)
        {
            if (group == null) 
            {
                Debug.Log("IsTemplatePresentHere foudn no group, returned FALSE");
                return false;
            }
            return group.BeyondComponentsAt(here).Exists(bc => bc.template.name == t_name && bc.side == cs && bc.state != BC_State.Ghost) ;
        }

        public static void SetCanPlaceObjectColour(BeyondComponent bc)
        {
            Renderer r = bc.gameObject.GetComponent<Renderer>();
            //r.material.color = (ConstraintController.CanPlace(bc) ? Color.green : Color.red);
            r.material.color = (ConstraintController.CanSnapTo(bc, bc.beyondGroup , bc.groupPosition) ? Color.green : Color.red);
        }

        /*
        * ============================================================
        * CONSTRAINTS CHECKING
        * ============================================================
        */

        // Check this BC's constraints (optionally in a group) 
        public static bool CheckConstraints(BeyondComponent bc , Constraints c = null , BeyondGroup bg = null , Vector3Int? optionalGroupPos = null)
        {
            Constraints constraints = (c == null ? bc.template.constraints : c) ;
            //Debug.Log(String.Format("Checking constraint on {0} of group {1}: {2}" , bc.name , (bg==null ? "null" : bg.name) , Constraints.ShowConstraints(constraints)));
            //int i=0;
            switch(constraints.operation)
            {
                case "OR":
                    //if (i++>1000) {Debug.Log("OR just exploded"); return false;}
                    return OrConstraints(bc , bg , constraints.constraintsList , optionalGroupPos) ;
                case "AND":
                    //if (i++>1000) {Debug.Log("AND just exploded"); return false;}
                    return AndConstraints(bc , bg , constraints.constraintsList , optionalGroupPos) ;
                case "TOPCLEAR":
                    //if (i++>1000) {Debug.Log("TOPCLEAR just exploded"); return false;}
                    return TopClear(bc , constraints.mask);
                case "ALLCLEAR":
                    //if (i++>1000) {Debug.Log("ALLCLEAR just exploded"); return false;}
                    return AllClear(bc , constraints.mask);
                case "BASEIN":
                    //if (i++>1000) {Debug.Log("BASEIN just exploded"); return false;}
                    return BaseInTerrain(bc);
                case "NEEDSONE":
                    //if (i++>1000) {Debug.Log("NEEDSONE just exploded"); return false;}
                    return NeedsOne(bc , bg , constraints.templateNamesList, constraints.offsetsList, constraints.cellSides , optionalGroupPos);
                case "NEEDSALL":
                    //if (i++>1000) {Debug.Log("BEEDSALL just exploded"); return false;}
                    return NeedsAll(bc , bg , constraints.templateNamesList, constraints.offsetsList, constraints.cellSides , optionalGroupPos);
                case "FIRSTINGROUP":
                    //if (i++>1000) {Debug.Log("FIRSTINGROUP just exploded"); return false;}
                    return FirstInGroup(bc , bg);
                default:
                    //if (i++>1000) {Debug.Log("CHeckConstraints just exploded"); return false;}
                    return false;
            }
        }

        private static bool OrConstraints(BeyondComponent bc , BeyondGroup bg , List<Constraints> lc , Vector3Int? optionalGroupPos = null) 
        {
            foreach (Constraints c in lc)
            {
                if (CheckConstraints(bc , c , bg , optionalGroupPos)) return true;
            }
            return false ;
        }

        private static bool AndConstraints(BeyondComponent bc , BeyondGroup bg , List<Constraints> lc , Vector3Int? optionalGroupPos = null)
        {
            foreach (Constraints c in lc)
            {
                if (!CheckConstraints(bc , c, bg , optionalGroupPos)) return false;
            }
            return true ;
        }

        // Is the bottom inside terrain by enough ?
        private static bool BaseInTerrain(BeyondComponent bc)
        {
            Vector3 BoxCast = bc.template.castBox ;
            Vector3 p = bc.transform.gameObject.transform.position ;
            Quaternion q = bc.transform.gameObject.transform.rotation ;
            
            // Cast 4 boxes at each corner of the bottom of the object
            // Their centres are: p + BoxCast in both X and Z, plus FoundationInTerrainBy / 2
            for (float i = -1; i <= 2; i+=2)
            {
                for (float j = -1; j <= 2; j+=2)
                {
                    Vector3 point = (p + new Vector3((BoxCast.x - FoundationInTerrainBy/2) * i , - BoxCast.y + FoundationInTerrainBy/2 , (BoxCast.z - FoundationInTerrainBy/2) * j ))  ;
                    point = Utility.RotateAroundPoint(point , p , q) ;

                    //Debug.DrawLine(point + (Vector3.down * YOffset), new Vector3(point.x , point.y- FoundationInTerrainBy/2 , point.z) , Color.yellow , 0.25f);
                    // The height of the boxes' starting point must be offset by the template's half height (BoxCast.y)
                    if (Physics.BoxCast(point - (Vector3.down * BoxCast.y), BoxCast, Vector3.down, q, Mathf.Infinity , getTerrainMask()))
                    {
                        return false ;
                    }
                }
            }
            return true ;
        }

        private static bool TopClear(BeyondComponent bc, LayerMask mask)
        {
            Vector3 castFrom = bc.transform.position;
            castFrom.y = PlaceController.Instance.place.Height; // Cast from the highest possible altitude
            RaycastHit hitInfo;
            float rayLength = PlaceController.Instance.place.Height - bc.transform.position.y - bc.template.castBox.y*2;
            bool result = Physics.BoxCast(castFrom, bc.template.castBox, Vector3.down, out hitInfo, bc.transform.rotation, rayLength , mask);
            //if (result)  Debug.Log(bc.gameObject.name+" top is not clear");
            return !result;
        }

        private static bool AllClear(BeyondComponent bc , LayerMask mask)
        {
            Collider[] collidersHit = Physics.OverlapBox(bc.transform.position, bc.template.castBox, bc.transform.rotation, mask);
            // string debug_string="insideTerrain overlap boxes="; foreach (Collider c in collidersHit) debug_string += c.gameObject.name + ","; Debug.Log(debug_string);
            return (collidersHit.Length == 0);
        }

        private static bool NeedsOne(BeyondComponent bc, BeyondGroup bg , List<string> templatesList, List<Vector3Int> offsetsList, List<cellSide> cellSides , Vector3Int? optionalGroupPos = null)
        {
            if (bg==null) return false ;
            Vector3Int groupPos = (optionalGroupPos!=null) ? (Vector3Int)optionalGroupPos : bc.groupPosition ;
            foreach (string templateName in templatesList)
            {
                foreach (Vector3Int offset in offsetsList)
                {
                    foreach (cellSide side in cellSides)
                    {
                        if (IsTemplatePresentHere(bg , groupPos + offset , templateName , side))
                            return true ;
                    }
                }
            }
            Debug.Log("Check constraints NEEDSONE returned FALSE");
            return false ;
        }

        private static bool NeedsAll(BeyondComponent bc, BeyondGroup bg , List<string> templatesList, List<Vector3Int> offsetsList, List<cellSide> cellSides , Vector3Int? optionalGroupPos = null)
        {
            if (bg==null) return false ;
            Vector3Int groupPos = (optionalGroupPos!=null) ? (Vector3Int)optionalGroupPos : bc.groupPosition ;
            foreach (string templateName in templatesList)
            {
                foreach (Vector3Int offset in offsetsList)
                {
                    bool foundOne = false ;
                    foreach (cellSide side in cellSides)
                    {
                        if (IsTemplatePresentHere(bg , groupPos + offset , templateName , side))
                            foundOne = true ;
                    }
                    if (!foundOne)
                        return false ;
                }
            }
            return true ;
        }

        private static bool FirstInGroup(BeyondComponent bc , BeyondGroup bg)
        {
            if (bg==null) return true; // By definition, if there's no group, the bc fulfills this constraint
            if (bg.componentList.Count==1) return true ;
            return false ;
        }

    }
}
