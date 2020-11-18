using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class ElementPlacementController : MonoBehaviour
    {
        private KeyCode newFundationHotKey = KeyCode.U;
        private KeyCode newWallHotKey = KeyCode.I;
        private KeyCode newFloorHotKey = KeyCode.O;
        private KeyCode newWallholeHotKey = KeyCode.P;
        private BeyondComponent currentBC;
        private float mouseWheelRotation;
        public float groupSnapTolerance = 0.5f;
        public GameObject testSphere;
        [SerializeField]
        public Vector3 dragTo;
        Vector3 mousePosition;
        int nbObjectsPlaced=0;
        // TODO Dragging stuff: to sort out
        List<BeyondComponent> draggedBC;
        public BeyondGroup draggingGroup ;
        float ForwardOffsetBeforeDragging;

        Vector3Int lastGroupPosition ;
        int MaxDraggedObjectCount = 256;

        public void Awake()
        {
            draggedBC = new List<BeyondComponent>() ;
            draggingGroup = null;
        }

        void Update()
        {
            if (UIController.Instance.gameMode == gameMode.build)
            { //All this should only happens when gameMode=build
                HandleNewObjectHotkey();

                if ( currentBC != null)
                {
                    //bool wasRotated = RotateFromMouseWheel();
                    RotateFromMouseWheel();
                    //if (Input.mousePosition!=mousePosition || wasRotated)
                    //{ // only move placeable object when mouse has moved
                    mousePosition = Input.mousePosition;
                    MovePlaceableObjectToMouse();
                    //}

                    // Make the placeable red or green based on whether it can be placed
                    ConstraintController.SetCanPlaceObjectColour(currentBC) ;
                    Drag();
                    PlaceOnClic();
                }
            }
            else if (currentBC != null)
            { // Always destroy placeable object when we leave build mode
                Destroy(currentBC.gameObject);  Debug.Log("Going off build mode destroyed placeable object");
                currentBC = null;
            }
        }
        private void MovePlaceableObjectToMouse()
        {
            Ray ray1 = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hitInfo;

            // filter the raycast based on whether this template should be shown on terrain or not
            //LayerMask layerMask = (ConstraintController.ShowOnTerrain(currentBC.template) ? ConstraintController.getTerrainMask() : ConstraintController.getBuildingsMask()) ;
            Vector3 position ;

            // 1 - If we hit a building, show the Ghost on it
            if (Physics.Raycast(ray1, out hitInfo, UIController.Instance.forwardOffset, ConstraintController.getBuildingsMask()))
            {
                //Debug.Log("Move to mouse: hit a BUILDING");
                position = ConstraintController.PlaceGhost(currentBC, hitInfo.point, ConstraintController.getBuildingsMask());
            }

            // 2 - If we hit terrain, show the Ghost on it
            else if (Physics.Raycast(ray1, out hitInfo, UIController.Instance.forwardOffset , ConstraintController.getTerrainMask()))
            {
                //Debug.Log("Move to mouse: hit TERRAIN");
                position = ConstraintController.PlaceGhost(currentBC , hitInfo.point, ConstraintController.getTerrainMask());
            }

            // 3 - If we hit nothing, just make the object float in front of us at a distance of forwardOffset
            else
            { 
                //Debug.Log("Move to mouse: FLOAT");
                Ray ray2 = Camera.main.ScreenPointToRay(mousePosition);
                position = Camera.main.transform.position + ray2.direction * UIController.Instance.forwardOffset ;
            }

            // If I'm in a group, I'm snapped, so only move if I'm a bit far from my current position
            if (currentBC.beyondGroup==null || Vector3.Distance(position , currentBC.transform.position) > groupSnapTolerance)
            {
                currentBC.MoveGhost(position);
            }
            Snap();
        }

        private void Snap()
        {
            // Unset the group before trying to snap or weird things happen
            currentBC.unsetObjectGroup();

            // 1 - what groups are close to currentPlaceableObject ?
            BeyondGroup closestGroup ;
            findCloseGroups(currentBC , out closestGroup);
            if (closestGroup!=null) 
            {
                //Debug.Log("closestGroup"+closestGroup.name);

                // Find the centre where I should place the object based on the group's "root" position and the Vector3Int difference between there and here
                // 1 - where would the cell centre be, considering the offset of this placeable object's template
                Vector3 pointWithOffset = currentBC.transform.position - currentBC.template.pivotOffset;
                
                // 2 - calculate the difference between this point and the group's centre and apply the inverse rotation for the group so we can compare in a non-rotated way
                pointWithOffset = Utility.RotateAroundPoint(pointWithOffset , closestGroup.position , Quaternion.Inverse(closestGroup.rotation));

                // 3 - Obtain the equivalent Integer Vector3, which represents how many cells the object is from the group's centre
                Vector3Int diffInt2 = Vector3Int.RoundToInt(pointWithOffset - closestGroup.position);
                UIController.Instance.positionInGroup = diffInt2;

                // 4 - This is the position of the centre of the cell
                Vector3 snappedPosition = closestGroup.position + (Vector3)diffInt2;

                // 5 - Rotate it by the group's rotation for final result
                snappedPosition = Utility.RotateAroundPoint(snappedPosition , closestGroup.position , closestGroup.rotation) ;

                // TODO : move this constraint to ConstraintController
                // Test to see if foundation is above groud like in GetValidPositionFromMouse
                //TODO: Hardcode for the time being as I tweak GetPointOnLayer
                //TODO : I have removed minY so don't need the code below. See if that works. CanSnapTo should ideally prevent being inside terrain anyway
                //Vector3 point = ConstraintController.GetPointOnLayer(currentBC , currentBC.transform.position , ConstraintController.getTerrainMask()) ;
                //float minY = point.y ;
                //minY = 0;

                //if (snappedPosition.y>=minY)
                //{ // Snap in place only if object's top is above ground

                    // Not needed aymore-angle between the rotation of the group and the rotation of the object on the Y axis : float angle = Mathf.Abs(currentPlaceableObject.transform.rotation.eulerAngles.y - closestGroup.rotation.eulerAngles.y );

                    //if (ConstraintController.CanSnapToGroupHere(closestGroup , diffInt2 , currentBC.template , snappedPosition - pointWithOffset , currentBC.transform.rotation , out snapToSide))
                    if (ConstraintController.CanSnapTo(currentBC , closestGroup , diffInt2))
                    {
                        currentBC.SetBCinGroup(closestGroup, diffInt2);
                    }
                    // else Debug.Log("Can't snap here because of group constraints");
                //}
            }
            else
            {
                currentBC.unsetObjectGroup();
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
            if (currentBC == null)
            {
                //TemplateController.CreateObject(templateName , ref currentPlaceableObject , ref currentBC) ;
                // refactored into :
                currentBC = TemplateController.CreateObject(templateName) ;
            }
            else
            {
                Destroy(currentBC.gameObject);  Debug.Log("CreateNewPlaceableobject destroyed placeable object");
            }
        }

        private void Drag()
        {
            // Start draggin (we have no draggedBC)
            if (Input.GetMouseButtonDown(0) && draggedBC.Count==0)
                StartDrag();

            if (Input.GetMouseButton(0) && draggingGroup != null)
            { // we are dragging AND we have an actual group we're dragging
                /* Starting state :
                - draggedObjects contains a list of objects currently displayed as being dragged
                - currentPlaceableObject is the object we are dragging around
                */ 

                // 1 - Calculate the current position in the group for  currentPlaceableObject
                Vector3 objectPosition = currentBC.transform.position - currentBC.template.pivotOffset ;
                objectPosition = Utility.RotateAroundPoint(objectPosition , draggingGroup.position , Quaternion.Inverse(draggingGroup.rotation)) ;
                Vector3Int objectGroupPosition = Vector3Int.RoundToInt(objectPosition - draggingGroup.position);
                PushForwardOffset(); // Push ForwardOffset if dragging away

                //TODO : I had to add draggedBC.Count > 0 to correct an error on draggedBC being empty, but how could it be empty ?
                // If there was a change in position since last time we dragged, update
                if (objectGroupPosition != lastGroupPosition && draggedBC.Count > 0)
                {
                    Vector3 p1 ;
                    Vector3 p2 ;
                    int NbDimensions = draggedBC[0].DragDimensions(out p1 , out p2) ;

                    if ( NbDimensions == 1)
                    { // Dragging a line
                        Drag1D(objectGroupPosition, objectPosition, p1, p2);
                    }

                    if ( NbDimensions == 2)
                    { // Dragging a plane
                        Drag2D(objectGroupPosition, objectPosition, p1, p2);
                    }
                }

                /*
                Snap();
                // If Snapping succeeded, place this object, add it to draggedObjects, create a new thinggy 
                if (currentBC.beyondGroup!=null && ConstraintController.CanPlace(currentPlaceableObject))
                {
                    // TODO : this is where I should have logic to place walls in a line, other stuff in a surface, or even a friggin volume
                    string templateName = currentBC.template.name ;
                    string name = templateName + "_" + (nbObjectsPlaced++) ;

                    // Place the first Ghost = still green, still not collidin'
                    TemplateController.PlaceObject(ref currentPlaceableObject , name , BC_State.Ghost) ;

                    // Put current ghost in the list of dragged objects
                    draggedObjects.Add(currentPlaceableObject);
                    currentPlaceableObject = null ;
                    // Create a new placeable object that we are currently dragging
                    CreateNewPlaceableObject(templateName) ;
                }
                */
                // IF I already snapped, the current BC has a BeyondGroup set.

                // TODO Get where we currently are compared to the last object in the dragged list
                // If we are not in the same cell, put the current object in the dragged list and instantiate a new ghost object
                // TODO : dragging back : we should be able to destroy ghosts we are placing over, except the last one
            }
        }

        void PushForwardOffset()
        {
            float DistanceToBC = Vector3.Distance(currentBC.transform.position, Camera.main.transform.position) ;
            if (DistanceToBC > UIController.Instance.forwardOffset)
                UIController.Instance.SetForwardOffset(DistanceToBC+1);
        }

        void StartDrag()
        {
            ForwardOffsetBeforeDragging = UIController.Instance.forwardOffset;
            if (ConstraintController.CanPlace(currentBC))
            {
                string templateName = currentBC.template.name;
                string name = templateName + "_" + (nbObjectsPlaced++);

                // Place the first Ghost = still green, still not collidin'
                TemplateController.PlaceObject(currentBC, name, BC_State.Ghost);

                draggedBC.Add(currentBC);
                draggingGroup = currentBC.beyondGroup;
                lastGroupPosition = currentBC.groupPosition;

                currentBC = null;
                CreateNewPlaceableObject(templateName);
                // TODO : another hardcoded position. Not good. Yet, I must move the currentBC to where I started dragging or else I'm going to drag to some unknown place.
                currentBC.transform.position = draggedBC[0].transform.position;
                currentBC.transform.rotation = draggedBC[0].transform.rotation;

                // Instantiate a big bunch of placeable object based on what we are currently dragging
                for (int i = 0; i < MaxDraggedObjectCount; i++)
                {
                    BeyondComponent bc = TemplateController.CreateObject(templateName);
                    //TODO Better names, please
                    name = templateName + "_Ghost" + i;
                    bc.SetBCinGroup(draggingGroup, lastGroupPosition);
                    TemplateController.PlaceObject(bc, name, BC_State.Ghost);
                    bc.gameObject.SetActive(false);
                    draggedBC.Add(bc);
                }
            }
        }

        void Drag1D(Vector3Int objectGroupPosition , Vector3 objectPosition , Vector3 p1  , Vector3 p2)
        {
            dragTo = Utility.ClosestPointOnLine(p2, p2 + p1, objectPosition);
            testSphere.transform.position = dragTo;
            objectGroupPosition = Vector3Int.RoundToInt(dragTo - draggingGroup.position);
            /*
            if (objectGroupPosition != lastGroupPosition)
            { // even after projecting on a line, are we still in a new cell ?
              //TODO : Best to move the current placeable object on that position, to keep it on a line
                lastGroupPosition = objectGroupPosition;
                // The side decides which rotation we should use
                Vector3Int cellDiff = draggedBC[0].groupPosition - lastGroupPosition;
                Vector3Int oneCell = Vector3Int.RoundToInt((Vector3)cellDiff / cellDiff.magnitude);
                //Debug.Log("Drag(). cellDiff="+cellDiff);
                int i = 0;
                for (int j = 0; j < cellDiff.magnitude; j++)
                {
                    if (i++ < MaxDraggedObjectCount)
                    {
                        draggedBC[i].SetBCinGroup(draggingGroup, draggedBC[0].groupPosition + oneCell * j);
                        //draggedBC[i].transform.rotation = draggedBC[0].transform.rotation ;
                        //draggedBC[i].transform.position = draggedBC[0].transform.position + p1 * j ;
                        ConstraintController.SetCanPlaceObjectColour(draggedBC[i]);
                        draggedBC[i].gameObject.SetActive(true);
                    }
                }
                // Deactivate all ghosts that are not used in the area we are currently dragging
                if (i >= 0)
                {
                    for (int j = i + 1; j <= MaxDraggedObjectCount; j++)
                    {
                        draggedBC[j].gameObject.SetActive(false);
                    }
                }
            }
            */
        }

        void Drag2D(Vector3Int objectGroupPosition, Vector3 objectPosition, Vector3 p1, Vector3 p2)
        {
            Plane p = new Plane(p1, p2);
            Vector3 dragTo = p.ClosestPointOnPlane(objectPosition);
            // Correct ObjectGroupPosition so we are in the plane we are dragging in 
            objectGroupPosition = Vector3Int.RoundToInt(dragTo - draggingGroup.position);

            if (objectGroupPosition != lastGroupPosition)
            { // even after projecting on a plane, are we still in a new cell ?
                lastGroupPosition = objectGroupPosition;
                //Debug.Log("Dragging to a new position on a plane in the group: "+lastGroupPosition+" from draggedBC[0]: "+draggedBC[0].groupPosition);
                int Xsign = (lastGroupPosition.x >= draggedBC[0].groupPosition.x ? 1 : -1);
                int Zsign = (lastGroupPosition.z >= draggedBC[0].groupPosition.z ? 1 : -1);
                int i = 0;
                for (int x = 0; x <= Mathf.Abs(lastGroupPosition.x - draggedBC[0].groupPosition.x); x++)
                {
                    for (int z = 0; z <= Mathf.Abs(lastGroupPosition.z - draggedBC[0].groupPosition.z); z++)
                    { // go from the origin of the drag to the current position
                        if (x != 0 || z != 0)
                        {
                            Vector3Int currentPos = new Vector3Int(draggedBC[0].groupPosition.x + x * Xsign, 0, draggedBC[0].groupPosition.z + z * Zsign);
                            if (i++ < MaxDraggedObjectCount)
                            {
                                draggedBC[i].SetBCinGroup(draggingGroup, currentPos);
                                //Debug.Log("2 dimensions dragging - calling SetBCinGroup on " + draggedBC[i].name + " group:" + draggedBC[i].beyondGroup.name);
                                ConstraintController.SetCanPlaceObjectColour(draggedBC[i]);
                                draggedBC[i].gameObject.SetActive(true);
                            }
                        }
                    }
                }
                // Deactivate all ghosts that are not used in the area we are currently dragging
                if (i >= 0)
                {
                    for (int j = i + 1; j <= MaxDraggedObjectCount; j++)
                    {
                        draggedBC[j].gameObject.SetActive(false);
                    }
                }
            }
        }
        private void PlaceOnClic()
        {
            if (Input.GetMouseButtonUp(0))
            {
                //TODO : dragging stuff - make it better
                for (int i = 0; i < draggedBC.Count; i++)
                {
                    if (draggedBC[i].gameObject.activeSelf && ConstraintController.CanPlace(draggedBC[i]))
                    {
                        string name = currentBC.template.name + "_" + (nbObjectsPlaced++) ;
                        TemplateController.PlaceObject(draggedBC[i] , name) ;
                    }
                    else
                    {
                        Destroy(draggedBC[i].gameObject);
                    }
                }
                Destroy(currentBC.gameObject);
                draggingGroup = null ;
                draggedBC.Clear();
                UIController.Instance.SetForwardOffset(ForwardOffsetBeforeDragging); // reset ForwardOffset to what it was before dragging - less annoying
                /*
                This works for 1 clic :

                if (ConstraintController.CanPlace(currentPlaceableObject))
                {
                    string name = currentBC.template.name + "_" + (nbObjectsPlaced++) ;
                    TemplateController.PlaceObject(ref currentPlaceableObject , name) ;
                }
                else
                {
                    //Debug.Log("Destroyed currentPlaceableObject "+ currentPlaceableObject .GetInstanceID()+ " as I couldn't place it");
                    Destroy(currentPlaceableObject);
                }
                */
            }
        }

        private bool RotateFromMouseWheel()
        {
            if ((mouseWheelRotation  != Input.mouseScrollDelta.y) && !Input.GetKey(KeyCode.LeftControl))
            {
                mouseWheelRotation = Input.mouseScrollDelta.y;
                currentBC.transform.Rotate(Vector3.up, mouseWheelRotation * 5f);
                return true;
            }
            return false;
        }
    }
}