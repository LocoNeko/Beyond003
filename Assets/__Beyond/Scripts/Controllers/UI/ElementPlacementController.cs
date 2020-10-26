using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class ElementPlacementController : MonoBehaviour
    {
        public Terrain terrain;
        public GameObject Fundation_prefab;

        public LayerMask TerrainLayerMask;

        [SerializeField]
        private KeyCode newFundationHotKey = KeyCode.U;
        private KeyCode newWallHotKey = KeyCode.I;
        private KeyCode newLevelHotKey = KeyCode.O;
        private KeyCode newWallholeHotKey = KeyCode.P;
        private GameObject currentPlaceableObject;
        private BeyondComponent currentPlaceableBeyondComponent;
        private float mouseWheelRotation;
        float heightOffset = 0f;

        public float groupSnapTolerance = 0.2f;
        Vector3 mousePosition;
        int nbObjectsPlaced=0;

        void Update()
        {
            if (UIController.Instance.gameMode == gameMode.build)
            { //All this should only happens when gameMode=build
                HandleNewObjectHotkey();

                if (currentPlaceableObject != null)
                {
                    bool wasRotated = RotateFromMouseWheel();
                    bool heightWasAdjusted = MouseHeightOverride();
                    if (Input.mousePosition!=mousePosition || wasRotated || heightWasAdjusted)
                    { // only move placeable object when mouse has moved
                        mousePosition = Input.mousePosition;
                        MovePlaceableObjectToMouse();
                    }
                    // Make the placeable red or green based on whether it can be placed
                    //TODO: this belong inside the BeyondCOmponent itself
                    Renderer r = currentPlaceableObject.GetComponent<Renderer>();
                    r.material.color = (ConstraintController.CanPlace(currentPlaceableObject , heightOffset) ? Color.green : Color.red);
                    ReleaseIfClicked();
                }
            }
            else if (currentPlaceableObject != null)
            { // Always destroy placeable object when we leave build mode
                Destroy(currentPlaceableObject);
                currentPlaceableBeyondComponent = null;
            }
        }
        private void MovePlaceableObjectToMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            // TODO FIRST : I think this is were my wall ghost position fails and returns a bad position within the group it snaps to
            //TODO FIRST : Really not sure non-foundation should be positioned on terrain, what's the point ? They have to  snap !
            // INSTEAD -> Move them freely in 3D (with mouse wheel giving depth) and snap accordingly
            // CONCLUSION : this layer mask depends on the template being placed 

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 250f, ConstraintController.getTerrainMask()))
            {
                Vector3 pointHit = hitInfo.point;
                Vector3 position = ConstraintController.GetValidPositionFromMouse(currentPlaceableObject , pointHit , ref heightOffset);
                position.y += heightOffset;
                //If I'm snapped, only move if I'm a bit far from my current position
                // TODO : see hardcoded bit in MovePlaceableObject: Can I give better offsets for mouse positionning ?
                if (currentPlaceableBeyondComponent.beyondGroup!=null)
                {
                    if (Vector3.Distance(position , currentPlaceableObject.transform.position) >0.2f)
                    {
                        MovePlaceableObject(position);
                    }
                }
                else
                {
                    MovePlaceableObject(position);
                }
                // Unset the group before trying to snap or weird things happen
                currentPlaceableBeyondComponent.unsetObjectGroup();
                Snap();
            }
        }

        private void MovePlaceableObject(Vector3 p , Quaternion? r = null)
        {
            currentPlaceableObject.transform.position = p ;
            if (currentPlaceableBeyondComponent.template.name != "Foundation")
            { // TODO : very hardcoded. Can I give better offsets for mouse positionning ? put it back in MovePlaceableObjectToMouse ?
                currentPlaceableObject.transform.position += currentPlaceableBeyondComponent.template.pivotOffset;
            } 
            if (r != null)
            {
                currentPlaceableObject.transform.rotation = (Quaternion)r;
            }
        }

        private void Snap()
        {
            // 1 - what groups are close to currentPlaceableObject ?
            BeyondGroup closestGroup ;
            findCloseGroups(currentPlaceableBeyondComponent , out closestGroup);
            if (closestGroup!=null) 
            {
                //Debug.Log("closestGroup"+closestGroup.name);

                // Find the centre where I should place the object based on the group's "root" position and the Vector3Int difference between there and here
                // 1 - where would the cell centre be, considering the offset of this placeable object's template
                // TODO : this is already wrong :  walls rotate, and base on that the cell's centre is not that
                Vector3 pointWithOffset = currentPlaceableBeyondComponent.transform.position - currentPlaceableBeyondComponent.template.pivotOffset;
                
                // 2 - calculate the difference between this point and the group's centre and apply the inverse rotation for the group
                pointWithOffset = RotateAroundPoint(pointWithOffset , closestGroup.position , Quaternion.Inverse(closestGroup.rotation));
                /*
                The statement above should be equivalent to the 3 lines below
                Vector3 diff = pointWithOffset - closestGroup.position ;
                diff = Quaternion.Inverse(closestGroup.rotation) * diff ;
                pointWithOffset = diff + closestGroup.position ;
                */

                // 3 - Obtain the equivalent Integer Vector3, which represents how many cells the object is from the group's centre
                Vector3Int diffInt2 = Vector3Int.RoundToInt(pointWithOffset - closestGroup.position);
                UIController.Instance.positionInGroup = diffInt2;

                // 4 - This is the position of the centre of the cell
                Vector3 snappedPosition = closestGroup.position + (Vector3)diffInt2; //FIXME + currentPlaceableBeyondComponent.template.pivotOffset;
                Vector3 snappedPositionDebug = snappedPosition ;

                // 5 - Rotate it by the group's rotation for final result
                snappedPosition = RotateAroundPoint(snappedPosition , closestGroup.position , closestGroup.rotation) ;
                /*
                The statement above should be equivalent to the 3 lines below
                Vector3 direction = snappedPosition - closestGroup.position ;
                direction = closestGroup.rotation * direction ;
                snappedPosition = direction + closestGroup.position ;
                */

                //Debug.Log("snap to object in position "+diffInt2+" group rotation changed the position by: "+(snappedPositionDebug-snappedPosition));
                // TODO : move all constraints to ConstraintController

                // Test to see if foundation is above groud like in GetValidPositionFromMouse
                Vector3 point = ConstraintController.GetPointOnTerrain(currentPlaceableObject , currentPlaceableObject.transform.position) ;
                float minY = point.y ;

                if (snappedPosition.y>=minY)
                { // Snap in place only if object's top is above ground
                    cellSide snapToSide;

                    // TODO: Not needed aymore: angle between the rotation of the group and the rotation of the object on the Y axis
                    // float angle = Mathf.Abs(currentPlaceableObject.transform.rotation.eulerAngles.y - closestGroup.rotation.eulerAngles.y );

                    //TODO : unfortunately, this is wrong. the rotation of the placeable object is irrelevant: snap to the side from which the object is closest !
                    // Then we can pass snapToSide directly
                    // TODO : so we need a distance not an angle
                    if (ConstraintController.CanSnapToGroupHere(closestGroup , diffInt2 , currentPlaceableBeyondComponent.template , snappedPosition - pointWithOffset , out snapToSide))
                    {
                        currentPlaceableObject.transform.position = snappedPosition ;
                        currentPlaceableBeyondComponent.setObjectGroup(closestGroup ,diffInt2 , snapToSide);
                    }
                    /*
                    else
                    {
                        Debug.Log("Can't snap here because of group constraints");
                    }
                    */
                }
            }
            else
            {
                currentPlaceableBeyondComponent.unsetObjectGroup();
                UIController.Instance.positionInGroup = new Vector3Int(-999,-999,-999);
            }
        }

        private List<BeyondGroup> findCloseGroups(BeyondComponent bc , out BeyondGroup closestGroup)
        {
            List<BeyondGroup> result= new List<BeyondGroup>();
            // Find the groups that are close to this BeyondComponent
            Collider[] collidersInGroup = Physics.OverlapBox(bc.transform.position , bc.template.castBox + new Vector3(1f,1f,1f) * groupSnapTolerance , bc.transform.rotation , ConstraintController.getBuildingsMask()) ;
            closestGroup = null ;
            float minDistance = 0;

            foreach(Collider c in collidersInGroup)
            {
                BeyondComponent collided_bc = c.transform.GetComponent<BeyondComponent>() ;
                if (collided_bc!=null && collided_bc.beyondGroup!=null)
                {
                    result.Add(collided_bc.beyondGroup) ;
                    float distance = Vector3.Distance(bc.transform.position , collided_bc.transform.position) ;
                    if (closestGroup==null || distance < minDistance)
                    {
                        minDistance = distance ;
                        closestGroup = collided_bc.beyondGroup ;
                    }
                }
            }
            UIController.Instance.SetClosestGroup(closestGroup);

            return result ;
        }

        private void HandleNewObjectHotkey()
        {
            if (Input.GetKeyDown(newFundationHotKey))
            {
                CreateNewPlaceableObject("Foundation");
            }
            if (Input.GetKeyDown(newWallHotKey))
            {
                CreateNewPlaceableObject("Wall");
            }
            if (Input.GetKeyDown(newLevelHotKey))
            {
                CreateNewPlaceableObject("Level");
            }
            if (Input.GetKeyDown(newWallholeHotKey))
            {
                CreateNewPlaceableObject("Wallhole");
            }
        }

        private void CreateNewPlaceableObject(string templateName)
        {
            if (currentPlaceableObject == null)
            {
                Template template = TemplateController.Instance.templates[templateName];
                heightOffset=0;
                //TODO : this will be inside the template controller.
                currentPlaceableObject = Instantiate(template.prefab);
                //TODO : need to experiment with BoxColldier & trigger
                currentPlaceableObject.GetComponent<BoxCollider>().enabled = true;
                currentPlaceableBeyondComponent = currentPlaceableObject.AddComponent<BeyondComponent>();
                currentPlaceableBeyondComponent.setTemplate(template);
            }
            else
            {
                //Debug.Log("Destroyed currentPlaceableObject " + currentPlaceableObject.GetInstanceID() + " when i re-pressed the place key");
                Destroy(currentPlaceableObject);
            }
        }
        private void ReleaseIfClicked()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (ConstraintController.CanPlace(currentPlaceableObject , heightOffset))
                {
                    // Set the material back to the prefab's material to get rid of the green or red colours
                    currentPlaceableObject.GetComponent<Renderer>().material = TemplateController.prefabMaterial(currentPlaceableBeyondComponent.template);
                    // TODO : Really need to think hard about this: will the box collider as trigger really be a general case for all elements ?
                    currentPlaceableObject.GetComponent<BoxCollider>().isTrigger = false;
                    currentPlaceableObject.GetComponent<BoxCollider>().enabled = true;
                    currentPlaceableBeyondComponent.SetState(BC_State.Blueprint) ;
                    currentPlaceableObject.name = currentPlaceableBeyondComponent.template.name + "_" + (nbObjectsPlaced++);
                    // if the object was not snapped to a group, create a new group
                    if(currentPlaceableBeyondComponent.beyondGroup==null)
                    {
                        PlaceController.Instance.CreateNewBeyondGroup(currentPlaceableBeyondComponent);
                    }
                    currentPlaceableObject = null;
                    //snapped = false;
                    //TODO : If SHIFT is pressed, allow queuing of objects to be placed
                }
                else
                {
                    //Debug.Log("Destroyed currentPlaceableObject "+ currentPlaceableObject .GetInstanceID()+ " as I couldn't place it");
                    Destroy(currentPlaceableObject);
                }
            }
        }

        private bool RotateFromMouseWheel()
        {
            if ((mouseWheelRotation  != Input.mouseScrollDelta.y) && !Input.GetKey(KeyCode.LeftControl))
            {
                mouseWheelRotation = Input.mouseScrollDelta.y;
                currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 5f);
                return true;
            }
            return false;
        }

        private bool MouseHeightOverride()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.mouseScrollDelta.y>0)
                {
                    heightOffset += 0.1f;
                }
                else if (Input.mouseScrollDelta.y < 0)
                {
                    heightOffset -= 0.1f;
                }
                return true;
            }
            return false;
        }

        //TODO : I should really place those into another class, specifically for that typoe of manipulation
        public static Vector3 RotateAroundPoint(Vector3 p , Vector3 pivot , Quaternion r)
        {
            return r * (p - pivot) + pivot ;
        }

    }
}