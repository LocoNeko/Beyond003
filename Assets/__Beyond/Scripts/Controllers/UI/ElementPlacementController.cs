using Beyond;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beyond
{

    public class ElementPlacementController : MonoBehaviour
    {
        public Terrain terrain;
        int nbObjectsPlaced = 0;

        [SerializeField]
        private KeyCode newObjectHotKey = KeyCode.U;
        private KeyCode lowerTerrainHotKey = KeyCode.T;

        /* TODO : There are 3 states for any objects :
         * - floating around currentPlaceableObject
         * - TODO: placed as a blueprint (under construction)
         * - Placed for good
         */
        private GameObject currentPlaceableObject;
        private BeyondComponent currentPlaceableBeyondComponent;
        private float mouseWheelRotation;
        public float heightOffset = -1f;
        private bool canPlace;
        Vector3 mousePosition;
        public bool snapped;

        public GameObject testObjectPrefab;
        List<GameObject> testObjects = new List<GameObject>();

        void Update()
        {
            if (UIController.Instance.gameMode == gameMode.build)
            { //All this should only happens when gameMode=build
                HandleNewObjectHotkey();

                if (currentPlaceableObject != null)
                {
                    bool wasRotated = RotateFromMouseWheel();
                    bool heightWasAdjusted = mouseHeightOverride();
                    if (Input.mousePosition!=mousePosition || wasRotated || heightWasAdjusted)
                    { // only move placeable object when mouse has moved
                        mousePosition = Input.mousePosition;
                        MoveCurrentPlaceableObjectToMouse();
                    }
                    ReleaseIfClicked();
                }
            }
            else if (currentPlaceableObject != null)
            { // Always destroy placeable object when we leave build mode
                Destroy(currentPlaceableObject);
            }
        }

        private void MoveCurrentPlaceableObjectToMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 250f, PlaceController.Instance.terrainLayerMask))
            {
                Vector3 pointHit = hitInfo.point;
                float height = GetHeightForClearTop();
                // Here, allow a Y offset to be applied, to get base down or up a bit
                pointHit.y = height + heightOffset;
                currentPlaceableObject.transform.position = pointHit;
                SnapToPlacedObjects();
                // Now check whether base is inside terrain
                if (snapped)
                {
                    // is the snapped object's top clear of terrain ?
                    // TO DO : this doesn't work if the base is just at the centre of a peak : the height itself will be fine, but the terrain will pierce through
                    // So I need a function that checks whether a thin box located slightly above the object collides with terrain
                    // TODO : don't hardcode this 2.5f
                    //if (height - pointHit.y <= 2.5f)
                    if (height - currentPlaceableBeyondComponent.transform.position.y <= 2.5f)
                    {
                        setCanPlace(!ObjectInsideAnother());
                        //TODO Don't I need to unsnap when I can't place the object ?
                    }
                    else
                    {
                        setCanPlace(false);
                        snapped = false;
                    }
                }
                else
                {
                    if (BaseIsInsideTerrain() && !ObjectInsideAnother())
                    {
                        setCanPlace(true);
                    }
                    else
                    {
                        setCanPlace(false);
                    }
                }
            }
        }



        private float GetHeightForClearTop()
        {
            //TO DO : don't hardcode 10 here
            Vector3 point = currentPlaceableObject.transform.position + 10 * Vector3.up;
            RaycastHit hitInfo;
            Physics.BoxCast(point, currentPlaceableBeyondComponent.castBox, Vector3.down, out hitInfo, currentPlaceableObject.transform.rotation);
            return hitInfo.point.y;
        }

        private bool BaseIsInsideTerrain()
        {
            if (currentPlaceableBeyondComponent!=null)
            {
                foreach (Feature f in currentPlaceableBeyondComponent.features)
                { // Check all features tagged as "InTerrain"
                    if (f.tag == "InTerrain")
                    {
                        RaycastHit hitInfo;
                        if (Physics.Raycast(f.gameObject.transform.position, Vector3.down, out hitInfo, 50f, PlaceController.Instance.terrainLayerMask))
                        { // If I hit the terrain while casting a ray down, this can only mean I'm not inside it
                            //Debug.Log("Base is not inside terrain");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool ObjectInsideAnother()
        {
            if (currentPlaceableBeyondComponent!=null)
            {
                if (currentPlaceableBeyondComponent.objectsColliding.Count>0)
                {
                    //Debug.Log("Colliding with placed object " + currentPlaceableBeyondComponent.objectsCollidingString());
                }
                //Debug.Log("ObjectInsideAnother is "+ currentPlaceableBeyondComponent.collidingWithBuilding()+" collided with "+ currentPlaceableBeyondComponent.objectsCollidingString());
                return currentPlaceableBeyondComponent.collidingWithBuilding(false);
            }
            return false;
        }

        private void SnapToPlacedObjects()
        {
            /* 
            * 1 - Get all featuresColliding GameObjects
            * 2 - Get all their parent placedObjects
            * 3 - Go through all the Features of the placedObjects, ignoring those that we didn't collide with
            * = By PlacedObjects, we now have a list of Features, and we know they were collided with
            * 4 - Go through those features to get all canLinkTo that we find in 2+ features
            * 5 - get the corresponding neighbouring cell centres by using neighbourCentre(GameObject go , Vector3Int direction)
            */
            if (currentPlaceableBeyondComponent != null)
            {
                // 1 - Get all featuresColliding GameObjects
                IEnumerable<GameObject> featuresColliding = currentPlaceableBeyondComponent.collidingWithFeatures();
                // 2 - Get all their parent placedObjects
                HashSet<GameObject> collidedPlacedObjects = new HashSet<GameObject>();
                HashSet<Vector3> uniqueNeighbourCentres = new HashSet<Vector3>();
                Dictionary<Vector3,GameObject> placedObjectAtCentre = new Dictionary<Vector3, GameObject>();
                // Put the parent of the collided feature in collidedPlacedObjects
                foreach (GameObject featureCollided in featuresColliding)
                {
                    collidedPlacedObjects.Add(featureCollided.transform.parent.gameObject);
                }
                // 3 - Go through all the Features of the placedObjects, ignoring those that we didn't collide with
                foreach (GameObject collidedPlacedObject in collidedPlacedObjects)
                {
                    BeyondComponent bc = collidedPlacedObject.GetComponent<BeyondComponent>();
                    HashSet<Feature> featuresOfCollidedObject = new HashSet<Feature>();
                    HashSet<Vector3Int> uniqueNeighbour = new HashSet<Vector3Int>();
                    // Get a list of Features for this collidedPlacedObject
                    foreach (Feature f in bc.features)
                    {
                        if (featuresColliding.Contains<GameObject>(f.gameObject))
                        { // only keep features we collided with
                            featuresOfCollidedObject.Add(f);
                        }
                    }
                    // 4 - Go through those features to get all canLinkTo that we find in 2+ features
                    foreach (Feature f2 in featuresOfCollidedObject)
                    {
                        foreach (Vector3Int v in f2.canLinkTo)
                        {
                            foreach (Feature f3 in featuresOfCollidedObject)
                            {
                                if ((f2 != f3) && (f3.canLinkTo.Contains(v)))
                                {
                                    uniqueNeighbour.Add(v);
                                }
                            }
                        }
                    }
                    // 5 - get the corresponding neighbouring cell centres by using neighbourCentre(GameObject go, Vector3Int direction)
                    foreach (Vector3Int v2 in uniqueNeighbour)
                    {
                        Vector3 point = PlaceController.Instance.neighbourCentre(collidedPlacedObject, v2);
                        uniqueNeighbourCentres.Add(point) ;
                        if (!placedObjectAtCentre.ContainsKey(point))
                        {
                            placedObjectAtCentre.Add(point, collidedPlacedObject);
                        }
                    }
                }
                //Debug.Log(s);
                // 6 - Check the closest centre to the currentPlaceableObject
                if (collidedPlacedObjects.Count>0)
                {
                    float d = -1;
                    foreach (Vector3 candidateCentre in uniqueNeighbourCentres)
                    {
                        float d2 = Vector3.Distance(candidateCentre , currentPlaceableObject.transform.position);
                        if (d == -1 || d2 < d)
                        {
                            d = d2;
                            currentPlaceableObject.transform.position = candidateCentre;
                            currentPlaceableObject.transform.rotation = placedObjectAtCentre[candidateCentre].transform.rotation;
                            //TODO : should I really set the group here  ?
                            BeyondGroup group = placedObjectAtCentre[candidateCentre].GetComponent<BeyondComponent>().beyondGroup;
                            if (group!=null)
                            {
                                currentPlaceableBeyondComponent.setObjectGroup(group);
                            }
                        }
                    }
                    snapped = true;
                }
                else
                {
                    snapped = false;
                    currentPlaceableBeyondComponent.unsetObjectGroup();
                }
            }
        }

        private bool RotateFromMouseWheel()
        {
            if ((mouseWheelRotation  != Input.mouseScrollDelta.y) && !Input.GetKey(KeyCode.LeftControl))
            {
                mouseWheelRotation = Input.mouseScrollDelta.y;
                currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 2f);
                return true;
            }
            return false;
        }

        private bool mouseHeightOverride()
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
                heightOffset = Mathf.Clamp(heightOffset, -2.5f, 0f);
                return true;
            }
            return false;
        }


        private void ReleaseIfClicked()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    // Set the material back to the prefab's material to get rid of the green or red colours
                    currentPlaceableObject.GetComponent<Renderer>().material = TemplateController.prefabMaterial(currentPlaceableBeyondComponent.template);
                    // TODO : Really need to think hard about this: will the box collider as trigger really be a general case for all elements ?
                    currentPlaceableObject.GetComponent<BoxCollider>().isTrigger = false;
                    currentPlaceableObject.GetComponent<BoxCollider>().enabled = true;
                    currentPlaceableObject.name = "placedObject_" + (nbObjectsPlaced++);
                    if(currentPlaceableBeyondComponent.beyondGroup==null)
                    {
                        PlaceController pc = PlaceController.Instance;
                        pc.CreateNewBeyondGroup(currentPlaceableBeyondComponent);
                    }
                    currentPlaceableObject = null;
                    snapped = false;
                    //TODO : If SHIFT is pressed, allow queuing of objects to be placed
                }
                else
                {
                    Destroy(currentPlaceableObject);
                }
            }
        }

        private void HandleNewObjectHotkey()
        {
            if (Input.GetKeyDown(newObjectHotKey))
            {
                if (currentPlaceableObject == null)
                {
                    currentPlaceableObject = TemplateController.Instance.TestCreate("Fundation");
                    currentPlaceableBeyondComponent = currentPlaceableObject.GetComponent<BeyondComponent>();
                    setCanPlace(false);
                }
                else
                {
                    Destroy(currentPlaceableObject);
                }
            }
            // TO DO : This needs a lot of work, and to understand terrain resolution to properly translate World -> terrain height map 
            if (Input.GetKeyDown(lowerTerrainHotKey))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 500f, PlaceController.Instance.terrainLayerMask))
                {
                    int xRes = terrain.terrainData.heightmapResolution;
                    int yRes = terrain.terrainData.heightmapResolution;
                    Vector3 pointHit = hitInfo.point;
                    //testObject.transform.position = hitInfo.point;
                    int x = xRes - (int)hitInfo.point.x;
                    int y = yRes - (int)hitInfo.point.z;
                    //Debug.Log(String.Format("Just hit Terrain at coordinates [{0:0.##},{1:0.##}]", x,y));
                    /*
                    float[,] newHeights = new float [1,1];
                    newHeights[0,0] = 0.1f;
                    terrain.terrainData.SetHeights(x, y , newHeights);
                    */
                }
                /*
                float[,] heights = terrain.terrainData.GetHeights(0, 0, xRes, yRes);
                */
            }

        }

        private void setCanPlace(bool b)
        {
            canPlace = b;
            Renderer r = currentPlaceableObject.GetComponent<Renderer>();
            if (!canPlace)
            {
                r.material.color = Color.red;
            }
            else
            {
                r.material.color = Color.green;
            }
        }

    }
}