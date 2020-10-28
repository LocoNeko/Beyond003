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
        private KeyCode newFloorHotKey = KeyCode.O;
        private KeyCode newWallholeHotKey = KeyCode.P;
        private GameObject currentPlaceableObject;
        private BeyondComponent currentPlaceableBeyondComponent;
        private float mouseWheelRotation;

        public float groupSnapTolerance = 0.5f;
        Vector3 mousePosition;
        int nbObjectsPlaced=0;

        void Update()
        {
            if (UIController.Instance.gameMode == gameMode.build)
            { //All this should only happens when gameMode=build
                HandleNewObjectHotkey();

                if (currentPlaceableObject != null)
                {
                    //bool wasRotated = RotateFromMouseWheel();
                    RotateFromMouseWheel();
                    //if (Input.mousePosition!=mousePosition || wasRotated)
                    //{ // only move placeable object when mouse has moved
                        mousePosition = Input.mousePosition;
                        MovePlaceableObjectToMouse();
                    //}

                    // Make the placeable red or green based on whether it can be placed
                    UIController.Instance.SetCanPlaceObjectColour(currentPlaceableObject) ;
                    PlaceOnClic();
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
            Ray ray1 = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hitInfo;

            // filter the raycast based on whether this template should be shown on terrain or not
            LayerMask layerMask = (ConstraintController.ShowOnTerrain(currentPlaceableBeyondComponent.template) ? ConstraintController.getTerrainMask() : ConstraintController.getBuildingsMask()) ;
            Vector3 position ;

            if (Physics.Raycast(ray1, out hitInfo, UIController.Instance.forwardOffset , layerMask))
            {
                position = ConstraintController.GetValidTerrainPointForObject(currentPlaceableObject , hitInfo.point);
            }
            else
            { // If we don't hit terrain, just make the object float in front of us
                Ray ray2 = Camera.main.ScreenPointToRay(mousePosition);
                position = Camera.main.transform.position + ray2.direction * UIController.Instance.forwardOffset ;
            }

            // If I'm in a group, I'm snapped, so only move if I'm a bit far from my current position
            if (currentPlaceableBeyondComponent.beyondGroup==null || Vector3.Distance(position , currentPlaceableObject.transform.position) > groupSnapTolerance)
            {
                MovePlaceableObject(position);
            }
            Snap();
        }

        private void MovePlaceableObject(Vector3 p , Quaternion? r = null)
        {
            currentPlaceableObject.transform.position = p ;
              // TODO: clearly, the pivotOffset works well for Foundations average for Walls, badly for floors.
              // TODO: BECAUSE, I need a specific point per template to anchor the mouse to 
            currentPlaceableObject.transform.position += currentPlaceableBeyondComponent.template.pivotOffset + Vector3.up*0.5f;
            if (r != null)
            {
                currentPlaceableObject.transform.rotation = (Quaternion)r;
            }
        }

        private void Snap()
        {
            // Unset the group before trying to snap or weird things happen
            currentPlaceableBeyondComponent.unsetObjectGroup();

            // 1 - what groups are close to currentPlaceableObject ?
            BeyondGroup closestGroup ;
            findCloseGroups(currentPlaceableBeyondComponent , out closestGroup);
            if (closestGroup!=null) 
            {
                //Debug.Log("closestGroup"+closestGroup.name);

                // Find the centre where I should place the object based on the group's "root" position and the Vector3Int difference between there and here
                // 1 - where would the cell centre be, considering the offset of this placeable object's template
                Vector3 pointWithOffset = currentPlaceableBeyondComponent.transform.position - currentPlaceableBeyondComponent.template.pivotOffset;
                
                // 2 - calculate the difference between this point and the group's centre and apply the inverse rotation for the group so we can compare in a non-rotated way
                pointWithOffset = RotateAroundPoint(pointWithOffset , closestGroup.position , Quaternion.Inverse(closestGroup.rotation));

                // 3 - Obtain the equivalent Integer Vector3, which represents how many cells the object is from the group's centre
                Vector3Int diffInt2 = Vector3Int.RoundToInt(pointWithOffset - closestGroup.position);
                UIController.Instance.positionInGroup = diffInt2;

                // 4 - This is the position of the centre of the cell
                Vector3 snappedPosition = closestGroup.position + (Vector3)diffInt2; //FIXME + currentPlaceableBeyondComponent.template.pivotOffset;

                // 5 - Rotate it by the group's rotation for final result
                snappedPosition = RotateAroundPoint(snappedPosition , closestGroup.position , closestGroup.rotation) ;

                // TODO : move this constraint to ConstraintController
                // Test to see if foundation is above groud like in GetValidPositionFromMouse
                Vector3 point = ConstraintController.GetPointOnTerrain(currentPlaceableObject , currentPlaceableObject.transform.position) ;
                float minY = point.y ;

                if (snappedPosition.y>=minY)
                { // Snap in place only if object's top is above ground
                    cellSide snapToSide;

                    // Not needed aymore-angle between the rotation of the group and the rotation of the object on the Y axis : float angle = Mathf.Abs(currentPlaceableObject.transform.rotation.eulerAngles.y - closestGroup.rotation.eulerAngles.y );

                    // Then we can pass snapToSide directly
                    if (ConstraintController.CanSnapToGroupHere(closestGroup , diffInt2 , currentPlaceableBeyondComponent.template , snappedPosition - pointWithOffset , out snapToSide))
                    {
                        currentPlaceableObject.transform.position = snappedPosition ;
                        currentPlaceableBeyondComponent.setObjectGroup(closestGroup ,diffInt2 , snapToSide);
                    }
                    // else Debug.Log("Can't snap here because of group constraints");
                }
            }
            else
            {
                currentPlaceableBeyondComponent.unsetObjectGroup();
                // TODO : gros caca
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
            if (Input.GetKeyDown(newFloorHotKey))
            {
                CreateNewPlaceableObject("Floor");
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
                TemplateController.CreateObject(templateName , ref currentPlaceableObject , ref currentPlaceableBeyondComponent) ;
            }
            else
            {
                Destroy(currentPlaceableObject);
            }
        }
        private void PlaceOnClic()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (ConstraintController.CanPlace(currentPlaceableObject))
                {
                    string name = currentPlaceableBeyondComponent.template.name + "_" + (nbObjectsPlaced++) ;
                    TemplateController.PlaceObject(ref currentPlaceableObject , name) ;
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

        //TODO : I should really place those into another class, specifically for that type of manipulation
        public static Vector3 RotateAroundPoint(Vector3 p , Vector3 pivot , Quaternion r)
        {
            return r * (p - pivot) + pivot ;
        }

    }
}