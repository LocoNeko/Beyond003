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

        public static LayerMask getBuildingsMask()
        {
            return getMask("Buildings");
        }

        public static bool layerIsInMask(int l , LayerMask lm)
        {
            int layerMask = 1 << l;
            return (layerMask == lm.value);
        }

        public static bool CanPlace(GameObject go , float heightOffset)
        {
            if (go == null) return false;
            BeyondComponent bc = go.GetComponent<BeyondComponent>();
            if (bc==null) return false;
            if (bc.template.name=="Foundation")
            {
                // 1 - Foundations must be partially inside terrain
                if (!BaseIsInTerrain(go.transform.position , bc.template.castBox , go.transform.rotation , heightOffset)) return false;
            }
            else
            {
                // 2 -All non-foundations objects must be above terrain - return false immediately if they're not
                if(bc.insideTerrain()) return false ;
            }

            //3 - All non-foundations must be snapped to another building part
            if (bc.template.name!="Foundation" && bc.beyondGroup==null)
            {
                return false ;
            }

            //4 - Object can't collide with other objects in different group 
            return !bc.collidingWithBuilding();

            //5 - Think of moveable objects later (they're not in groups, so they don't snap = constraints are easier)
        }

        //IMPORTANT : go.transform.position cannot be used since we're trying to place the GameObject through this method
        //rotation is fine (even if we just created the go, rotation will just be Quaternion.Identity)
        public static Vector3 GetValidPositionFromMouse(GameObject go , Vector3 pointOnTerrain , ref float heightOffset)
        {
            if (go == null) return pointOnTerrain;
            BeyondComponent bc = go.GetComponent<BeyondComponent>();
            if (bc==null) return pointOnTerrain;

            // As a rule, the result is the same as the pointOnTerrain, we are just applying some filter below
            Vector3 result = pointOnTerrain ;
            
            if (bc.template.name=="Foundation")
            {
                // A foundation should never have a negative height offset or it would go into the ground
                heightOffset = Mathf.Max(heightOffset , 0);
                result = GetPointOnTerrain(go , pointOnTerrain) ;
            }
            // If GO is not a foundation, it has to snap with something so we don't care about Terrain
            return result;
        }

        // given a point, get an object's postion above or below it so that the object is exactly on the Terrain
        public static Vector3 GetPointOnTerrain(GameObject go , Vector3 point)
        {
            Vector3 result = point ;

            BeyondComponent bc = go.GetComponent<BeyondComponent>();
            if (bc==null) return point;

            result.y = PlaceController.Instance.place.Height ; // Cast from the highest possible altitude
            RaycastHit hitInfo;
            Physics.BoxCast(result, bc.template.castBox, Vector3.down, out hitInfo, go.transform.rotation , Mathf.Infinity, getTerrainMask()); //TODO - is this better ? : Physics.BoxCast(point, bc.template.castBox, Vector3.down, out hitInfo, go.transform.rotation);
            // Half the height of the object is bc.template.castBox.y
            result.y = hitInfo.point.y - bc.template.castBox.y;

            return result ;
        }

        // Is the bottom inside terrain by enough ?I also need to take the current heightOffset into accunt
        public static bool BaseIsInTerrain(Vector3 p , Vector3 BoxCast , Quaternion q , float YOffset)
        {
            // Cast 4 boxes at each corner of the bottom of the object
            // Their centres are: p + BoxCast in both X and Z, plus FoundationInTerrainBy / 2
            for (float i = -1; i <= 2; i+=2)
            {
                for (float j = -1; j <= 2; j+=2)
                {
                    Vector3 point = (p + new Vector3((BoxCast.x - FoundationInTerrainBy/2) * i , - BoxCast.y + FoundationInTerrainBy/2 + YOffset , (BoxCast.z - FoundationInTerrainBy/2) * j ))  ;
                    point = ElementPlacementController.RotateAroundPoint(point , p , q) ;
                    /*
                    The line above should be equivalent to the 3 lines below
                    Vector3 direction = point - p ; // In order to apply the rotation, we need to find a vector from p to this new point
                    direction = q * direction ; // we can then rotate that direction
                    point = direction + p; // and apply it back, now fully rotated, to the original point p
                    */

                    Debug.DrawLine(point + (Vector3.down * YOffset), new Vector3(point.x , point.y- FoundationInTerrainBy/2 , point.z) , Color.yellow , 0.25f);

                    // The height of the boxes' starting point must be offset by their height (BoxCast.y) and YOffset
                    RaycastHit hitInfo;
                    if (Physics.BoxCast(point - (Vector3.down * (BoxCast.y - YOffset)), BoxCast, Vector3.down, out hitInfo, q, Mathf.Infinity , getTerrainMask()))
                    {
                        return false ;
                    }
                }
            }
            return true ;
        }

        public static bool CanSnapToGroupHere(BeyondGroup group , Vector3Int here , Template t , Vector3 dFromPivot , out cellSide sts)
        {
            if (t.name=="Foundation")
            {
                sts = cellSide.Down ; // Foundations always snap down
                // must not have the same object (Foundation) here
                if (IsTemplatePresentHere(group , here, "Foundation")) return false ;
                // Must have a neighbouring Foundation in X or Z, same Y
                if (IsTemplatePresentHere(group , new Vector3Int(here.x-1 , here.y , here.z), "Foundation")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x+1 , here.y , here.z), "Foundation")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y , here.z-1), "Foundation")) return true ;
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y , here.z+1), "Foundation")) return true ;
                Debug.Log("No neighbouring foundation");
                return false;
            }
            if (t.name=="Wall")
            {
                sts = BeyondComponent.closestSide(dFromPivot) ;
                if (sts==cellSide.Up || sts==cellSide.Down)
                {
                    sts=cellSide.Left;
                }

                // There must be a foundation to place a Wall
                if (IsTemplatePresentHere(group , here, "Foundation")) 
                {
                    // Can't place a wall if there's already one in the same cell and the same side
                    if (IsTemplatePresentHere(group , here, "Wall" , sts))
                    {
                        return false ;
                    } 
                    return true ; 
                }
            }
            if (t.name=="Wallhole")
            {
                sts = BeyondComponent.closestSide(dFromPivot) ;
                if (sts==cellSide.Up || sts==cellSide.Down)
                {
                    sts=cellSide.Left;
                }

                // There must be a foundation to place a Wall
                if (IsTemplatePresentHere(group , here, "Foundation")) 
                {
                    // Can't place a wall if there's already one in the same cell and the same side
                    if (IsTemplatePresentHere(group , here, "Wall" , sts))
                    {
                        return false ;
                    } 
                    return true ; 
                }
            }
            if (t.name=="Level")
            {
                sts = cellSide.Down ;
                // must not have the same object (Level) here
                if (IsTemplatePresentHere(group , here, "Level")) return false ;

                // There must be a Wall 3 cells below
                if (IsTemplatePresentHere(group , new Vector3Int(here.x , here.y-3 , here.z), "Wall")) 
                {
                    return true ; 
                }
            }
            sts = cellSide.Down ; // because I need a default
            return false ;
        }

        public static bool IsTemplatePresentHere(BeyondGroup group , Vector3Int here , string t_name)
        {
            return group.BeyondComponentsAt(here).Exists(bc => bc.template.name == t_name) ;
        }

        public static bool IsTemplatePresentHere(BeyondGroup group , Vector3Int here , string t_name , cellSide cs)
        {
            return group.BeyondComponentsAt(here).Exists(bc => bc.template.name == t_name && bc.side == cs) ;
        }

        //TODO
        private static bool TopClear(BeyondComponent bc)
        {
            throw new NotImplementedException();
        }
        private static bool BaseInput(BeyondComponent bc , float depth)
        {
            throw new NotImplementedException();
        }

        private static bool AllClear(BeyondComponent bc)
        {
            throw new NotImplementedException();
        }

        private static bool NeedsOne(BeyondComponent bc, List<Template> templatesList, List<Vector3Int> offsetsList, List<int> cellSides)
        {
            throw new NotImplementedException();
        }

        private static bool NeedsAll(BeyondComponent bc, List<Template> templatesList, List<Vector3Int> offsetsList, List<int> cellSides)
        {
            throw new NotImplementedException();
        }

    }
}
