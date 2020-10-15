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
        public LayerMask groundMask;
        public LayerMask buildingMask;

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
        float heightOffset = -0.1f;
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
                    // Make the placeable red or green based on whether it can be placed
                    Renderer r = currentPlaceableObject.GetComponent<Renderer>();
                    r.material.color = (canPlace() ? Color.green : Color.red);
                    ReleaseIfClicked();
                }
            }
            else if (currentPlaceableObject != null)
            { // Always destroy placeable object when we leave build mode
                //Debug.Log("Destroyed currentPlaceableObject " + currentPlaceableObject.GetInstanceID() + " when leaving build mode");
                Destroy(currentPlaceableObject);
            }
        }

        private void MoveCurrentPlaceableObjectToMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 250f, groundMask))
            {
                Vector3 pointHit = hitInfo.point;
                float height = GetHeightForClearTop();
                // Here, allow a Y offset to be applied, to get base down or up a bit
                pointHit.y = height + heightOffset;
                currentPlaceableObject.transform.position = pointHit;
                SnapToPlacedObjects();
            }
        }

        public bool canPlace()
        {
            //TODO : not all placeableObjects need their base to be inside terrain, so I need to do better than this
            // Probably, canPlace() is a templateController method
            if (snapped)
            {
                // is the snapped object's top clear of terrain ?
                // TO DO : this doesn't work if the base is just at the centre of a peak : the height itself will be fine, but the terrain will pierce through
                // So I need a function that checks whether a thin box located slightly above the object collides with terrain
                //TODO : on occasions, this can be off by just a tiny bit (I got 0.2500005f once and couldn't snap)
                if (GetHeightForClearTop() - currentPlaceableBeyondComponent.transform.position.y <= 0.250001f)
                {
                    return !ObjectInsideAnother(false);
                }
            }
            else
            {
                return BaseIsInsideTerrain() && !ObjectInsideAnother();
            }
            return false;
        }


        private float GetHeightForClearTop()
        {
            //TO DO : don't hardcode 10 here,I should do better than this
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
                        if (Physics.Raycast(f.gameObject.transform.position, Vector3.down, out hitInfo, 50f, groundMask))
                        { // If I hit the terrain while casting a ray down, this can only mean I'm not inside it
                            //Debug.Log("Base is not inside terrain");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool ObjectInsideAnother(bool checkSameGroup=true)
        {
            if (currentPlaceableBeyondComponent!=null)
            {
                if (currentPlaceableBeyondComponent.objectsTriggered.Count>0)
                {
                    //Debug.Log("Colliding with placed object " + currentPlaceableBeyondComponent.objectsCollidingString());
                }
                //Debug.Log("ObjectInsideAnother is "+ currentPlaceableBeyondComponent.collidingWithBuilding(checkSameGroup) +" collided with "+ currentPlaceableBeyondComponent.objectsCollidingString());
                return currentPlaceableBeyondComponent.collidingWithBuilding(checkSameGroup);
            }
            return false;
        }

        private void SnapToPlacedObjects()
        {
            /* 
            * 1 - Get all featuresGameObjectsColliding GameObjects
            * 2 - Get all their parent placedObjects
            * 3 - Go through all the Features of the placedObjects, ignoring those that we didn't collide with
            * = By PlacedObjects, we now have a list of Features, and we know they were collided with
            * 4 - Go through those features to get all canLinkTo that we find in 2+ features
            * 5 - get the corresponding neighbouring cell centres by using neighbourCentre(GameObject go , Vector3Int direction)
            */
            if (currentPlaceableBeyondComponent != null)
            {
                // 1 - Get all featuresGameObjectsColliding GameObjects
                IEnumerable<GameObject> featuresGameObjectsColliding = currentPlaceableBeyondComponent.collidingWithFeatureGameObjects();

                // 2 - Get all their parent placedObjects
                HashSet<GameObject> collidedPlacedObjects = new HashSet<GameObject>();
                HashSet<Vector3> uniqueNeighbourCentres = new HashSet<Vector3>();
                Dictionary<Vector3,GameObject> placedObjectAtCentre = new Dictionary<Vector3, GameObject>();

                // Put the parent GameObject of the collided feature's GameObject in collidedPlacedObjects
                foreach (GameObject featureCollided in featuresGameObjectsColliding)
                {
                    collidedPlacedObjects.Add(featureCollided.transform.parent.gameObject);
                }

                // 3 - Go through all the Features of the placedObjects, ignoring those that we didn't collide with
                foreach (GameObject collidedPlacedObject in collidedPlacedObjects)
                {
                    BeyondComponent bc = collidedPlacedObject.GetComponent<BeyondComponent>();
                    HashSet<Feature> featuresOfCollidedObject = new HashSet<Feature>();
                    // Get a list of Features for this collidedPlacedObject
                    foreach (Feature f in bc.features)
                    {
                        if (featuresGameObjectsColliding.Contains<GameObject>(f.gameObject))
                        { // only keep features we collided with
                            featuresOfCollidedObject.Add(f);
                        }
                    }

                    HashSet<Vector3Int> uniqueNeighbour = getUniqueNeighbours(featuresGameObjectsColliding, collidedPlacedObject);

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

                // 6 - Check the closest centre to the currentPlaceableObject
                if (collidedPlacedObjects.Count>0)
                {
                    float d = -1;
                    Vector3 currentPosition = currentPlaceableObject.transform.position;
                    Quaternion currentRotation = currentPlaceableObject.transform.rotation;
                    foreach (Vector3 candidateCentre in uniqueNeighbourCentres)
                    {
                        float d2 = Vector3.Distance(candidateCentre , currentPosition);
                        if (d == -1 || d2 < d)
                        {
                            d = d2;
                            currentPosition = candidateCentre;
                            currentRotation = placedObjectAtCentre[candidateCentre].transform.rotation;
                            //TODO : should I really set the group here  ?
                            BeyondGroup group = placedObjectAtCentre[candidateCentre].GetComponent<BeyondComponent>().beyondGroup;
                            if (group!=null)
                            {
                                currentPlaceableBeyondComponent.setObjectGroup(group);
                            }
                            snapped = true;
                        }
                    }
                    // We have snapped, we can move and rotate accordingly.
                    // TODO : set the gorup here instead ?
                    if (snapped)
                    {
                        currentPlaceableObject.transform.position = currentPosition;
                        currentPlaceableObject.transform.rotation = currentRotation;
                    }
                }
                else
                {
                    snapped = false;
                    currentPlaceableBeyondComponent.unsetObjectGroup();
                }
            }
        }

        //TODO : This method is much more complex than that, as some Object might give links based on only one feature (e.g. I can snap a wall to another based on just 1)
        public HashSet<Vector3Int> getUniqueNeighbours(IEnumerable<GameObject> featuresGameObjectsColliding , GameObject collidedPlacedObject)
        {
            HashSet<Vector3Int> result = new HashSet<Vector3Int>();
            BeyondComponent bc = collidedPlacedObject.GetComponent<BeyondComponent>();
            HashSet<Feature> features = new HashSet<Feature>();

            // Get a list of Features for this collidedPlacedObject
            foreach (Feature f in bc.features)
            {
                if (featuresGameObjectsColliding.Contains<GameObject>(f.gameObject))
                { // only keep features we collided with
                    features.Add(f);
                }
            }

            // Go through those features to get all canLinkTo that we find in 2+ features
            // TODO THis is wrong, as we've seen some templates are happy with just one features, some don't like vertical snapping, etc
            foreach (Feature f2 in features)
            {
                foreach (Vector3Int v in f2.canLinkTo)
                {
                    // Don't check up and down for fundations
                    // TODO: so all this will depend on the template of what we are trying to snap: fundations don't snap vertically
                    if (v.y==0)
                    {
                        foreach (Feature f3 in features)
                        {
                            if ((f2 != f3) && (f3.canLinkTo.Contains(v)))
                            {
                                //TODO : this is great but insufficient. Another group COULD have an object here preventing placement !
                                BeyondGroup bg = bc.beyondGroup;
                                if (bg.hasBeyondComponentAtCoordinate(PlaceController.Instance.neighbourCentre(collidedPlacedObject, v)))
                                {
                                    Debug.Log("Group " + bg.name + " already has an object here");
                                }
                                else
                                {
                                    result.Add(v);
                                }
                            }
                        }
                    }
                }
            }
            return result;
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
                //TODO : clamp based on placebaleobject's height
                heightOffset = Mathf.Clamp(heightOffset, -0.2f, 0f);
                return true;
            }
            return false;
        }


        private void ReleaseIfClicked()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace())
                {
                    // Set the material back to the prefab's material to get rid of the green or red colours
                    currentPlaceableObject.GetComponent<Renderer>().material = TemplateController.prefabMaterial(currentPlaceableBeyondComponent.template);
                    // TODO : Really need to think hard about this: will the box collider as trigger really be a general case for all elements ?
                    currentPlaceableObject.GetComponent<BoxCollider>().isTrigger = false;
                    currentPlaceableObject.GetComponent<BoxCollider>().enabled = true;
                    currentPlaceableObject.name = "placedObject_" + (nbObjectsPlaced++);
                    if(currentPlaceableBeyondComponent.beyondGroup==null)
                    {
                        PlaceController.Instance.CreateNewBeyondGroup(currentPlaceableBeyondComponent);
                    }
                    currentPlaceableObject = null;
                    snapped = false;
                    //TODO : If SHIFT is pressed, allow queuing of objects to be placed
                }
                else
                {
                    //Debug.Log("Destroyed currentPlaceableObject "+ currentPlaceableObject .GetInstanceID()+ " as I couldn't place it");
                    Destroy(currentPlaceableObject);
                }
            }
        }

        private void HandleNewObjectHotkey()
        {
            if (Input.GetKeyDown(newObjectHotKey))
            {
                createNewPlaceableObject();
            }
            if (Input.GetKeyDown(lowerTerrainHotKey))
            {
                lowerTerrain();
            }
        }

        private void createNewPlaceableObject()
        {
            if (currentPlaceableObject == null)
            {
                currentPlaceableObject = TemplateController.Instance.CreateGameObject("Fundation");
                currentPlaceableBeyondComponent = currentPlaceableObject.GetComponent<BeyondComponent>();
            }
            else
            {
                //Debug.Log("Destroyed currentPlaceableObject " + currentPlaceableObject.GetInstanceID() + " when i re-pressed the place key");
                Destroy(currentPlaceableObject);
            }
        }

        private void lowerTerrain()
        {
            // TO DO : This needs a lot of work, and to understand terrain resolution to properly translate World -> terrain height map 

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 500f, groundMask))
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
}