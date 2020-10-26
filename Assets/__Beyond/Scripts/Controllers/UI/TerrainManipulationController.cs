using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public class TerrainManipulationController : MonoBehaviour
    {
        public GameObject testObject;
        public GameObject spherePrefab;

        public Terrain terrain;
        public GameObject selectionTerrain_prefab;
        public bool insideSelection ;
        Vector3 mousePosition;
        TerrainData terrainData;
        KeyCode terrainHotKey = KeyCode.T;
        Vector3[] points;
        GameObject selectionCube ;
        Bounds selectionBounds;
        int selectionStage;
        List<GameObject> spheres;
        HashSet<Vector2Int> terrainPoints;

        void Awake()
        {
            terrainData = terrain.terrainData;
            selectionStage=0;
            points = new Vector3[4];
            selectionCube = Instantiate(selectionTerrain_prefab) ;
            spheres = new List<GameObject>();
            terrainPoints = new HashSet<Vector2Int>();

        }

        // Update is called once per frame
        void Update()
        {
            HandleKeyDown();

            if (selectionStage!=0 && Input.mousePosition!= mousePosition)
            { // only move placeable object when mouse has moved
                if (selectionCube==null)
                {
                    selectionCube.SetActive(true);
                }
                mousePosition = Input.mousePosition;
                PointOnTerrain();
            }
            HandleClick();
        }

        private void HandleKeyDown()
        {
            if (Input.GetKeyDown(terrainHotKey))
            {
                if (selectionStage==0)
                {
                    selectionCube.SetActive(true) ;
                    selectionStage = 1 ;
                }
                else
                    selectionStage = 0 ;
            }
        }

        public void PointOnTerrain()
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 250f, ConstraintController.getTerrainMask()))
            {
                points[selectionStage-1] = hitInfo.point ;
                if (hitInfo.point.y> points[0].y)
                { // move first point to new height
                    points[0].y = hitInfo.point.y ;
                }
                if (selectionStage>1)
                { // All points need to be at the same height
                    points[selectionStage-1].y = points[0].y ;
                }

                UpdateSelectionCube(selectionStage) ;

                //Debug.Log("hitpoint="+hitInfo.point+", TerrainPoistion="+TerrainPosition(hitInfo.point)+" "+PositionTerrain(TerrainPosition(hitInfo.point)));
                testObject.transform.position = hitInfo.point;
                if (selectionStage==4)
                {
                    insideSelection = isInsideSelection(hitInfo.point);
                }
            }
        }


        private void HandleClick()
        {
            if(Input.GetMouseButtonDown(0))
            {
                if (selectionStage==1)
                {
                    //place first point
                    UpdateSelectionCube(1) ;
                    selectionStage++;
                }
                else if (selectionStage==2)
                {
                    //place second point
                    UpdateSelectionCube(2);
                    selectionStage++;
                }
                else if (selectionStage==3)
                {
                    UpdateSelectionCube(3);

                    // Get all points in the terrain heightmap that are inside the selection or neighbouring such a point
                    Vector3 firstPointPosition = selectionCube.transform.position;
                    firstPointPosition.y = points[0].y ;
                    Vector2Int firstTerrainPoint = TerrainPosition(firstPointPosition);
                    // TODO: Check if I need 2 lists : one for points inside the selection, one for neighbours of those points
                    terrainPoints.Clear();
                    PopulateTerrainPoints(firstTerrainPoint);


                    float lowestY = PlaceController.Instance.place.Height;
                    foreach (Vector2Int p in terrainPoints)
                    {
                        float[,] r = terrainData.GetHeights(p.x, p.y, 1, 1);
                        if (r[0,0]* terrainData.size.y < lowestY)
                        {
                            lowestY = r[0,0]* terrainData.size.y ;
                        }
                        Debug.Log(String.Format("[{0}:{1}]Height found = {2}" , p.x , p.y ,  (r[0, 0] * terrainData.size.y)));
                    }
                    Debug.Log("Lowest Y="+lowestY);
                    selectionStage = 4 ;
                }
                else if (selectionStage==4)
                {// reset all with a click
                    selectionCube.SetActive(false);
                    selectionStage=0;
                }
            }
        }

        private void PopulateTerrainPoints(Vector2Int point)
        {
            Debug.Log(string.Format("PopulateTerrainPoints [{0}:{1}]",point.x,point.y));
            if (!terrainPoints.Contains(point))
            {
                Vector2 posXZ = PositionTerrain(point) ;
                Vector3 posXYZ = new Vector3(posXZ.x , points[0].y , posXZ.y) ;
                if (isInsideSelection(posXYZ))
                { // The point is inside the selection
                    terrainPoints.Add(point) ;
                    //GameObject sphere = Instantiate(spherePrefab);
                    //sphere.transform.position = posXYZ ;
                    //ChangeHeight(point , 3.0f);
                }
                else
                { // The point is outside the selection, but might be a neighbour
                    bool HasANeighbourInside = false ;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if(i!=0 || j!=0)
                            {
                                Vector2 neighbour_posXZ = PositionTerrain(new Vector2Int(point.x+i , point.y+j)) ;
                                Vector3 neighbour_posXYZ = new Vector3(neighbour_posXZ.x , points[0].y , neighbour_posXZ.y) ;
                                if (isInsideSelection(neighbour_posXYZ))
                                {
                                    HasANeighbourInside = true ;
                                }
                            }
                        }
                    }
                    if(HasANeighbourInside)
                    {
                        terrainPoints.Add(point) ;
                        //GameObject sphere = Instantiate(spherePrefab);
                        //sphere.transform.position = posXYZ ;
                        //ChangeHeight(point , 3.0f);
                    }
                    else
                    {
                        return ; // this point is too far
                    }
                }
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if(i!=0 || j!=0)
                        {
                            PopulateTerrainPoints(new Vector2Int(point.x+i , point.y+j)) ;
                        }
                    }
                }
            }
            return ; // The point was already in the set: return
        }

        private void ChangeHeight(Vector2Int point, float v)
        {
            float[,] heights = terrainData.GetHeights(point.x, point.y, 1, 1);
            float[,] newHeights = new float [1,1];
            newHeights[0,0] = v / terrainData.size.y;/*((heights[0, 0] * terrainData.size.y) - 0.1f) / terrainData.size.y;*/
            terrainData.SetHeights(point.x, point.y , newHeights);
        }

        public void UpdateSelectionCube(int stage)
        {
            if (stage==1)
            {
                selectionCube.transform.localScale = new Vector3(0.2f, (points[0].y-PlaceController.Instance.place.LowestY), 0.2f) ;
                Vector3 newPos = new Vector3(points[0].x,(points[0].y+PlaceController.Instance.place.LowestY)/2,points[0].z);
                selectionCube.transform.position = newPos ;
            }
            if (stage==2)
            {
                selectionCube.transform.localScale = new Vector3(Vector3.Distance(points[0] , points[1]), (points[0].y-PlaceController.Instance.place.LowestY), 0.2f) ;
                selectionCube.transform.position = new Vector3((points[1].x+points[0].x)/2 ,  (points[0].y+PlaceController.Instance.place.LowestY)/2 , (points[1].z+points[0].z)/2);
                float angle = Vector3.SignedAngle(Vector3.right , points[1]-points[0] , Vector3.up);
                selectionCube.transform.rotation = Quaternion.Euler(0,angle,0);
            }
            if (stage==3)
            {
                Vector3 v = points[1] - points[0] ; // The line between the first 2 points
                v = v.normalized;
                Ray ray2 = new Ray(points[0] , v) ; // ray from first point aiming at second point
                float d = Vector3.Cross(ray2.direction , points[2] - ray2.origin).magnitude ; // distance between the third point and that ray = width of the box

                float sign = ( (Vector3.SignedAngle(points[1]-points[0] , points[2]-points[0] , Vector3.up)< 0) ? 1 : -1) ; // on what side of the first line is the thrid point ?

                v = Quaternion.Euler(0,-90,0) * v ; // rotate -90 degrees to get the perpendicular vector

                selectionCube.transform.localScale = new Vector3(Vector3.Distance(points[0] , points[1]), (points[0].y-PlaceController.Instance.place.LowestY), d) ;
                selectionCube.transform.position = new Vector3((points[1].x+points[0].x)/2 ,  (points[0].y+PlaceController.Instance.place.LowestY)/2 , (points[1].z+points[0].z)/2) + (d/2)*v*sign;
                points[2] = points[1] + d * v * sign ;
                points[3] = points[0] + d * v * sign ;
                selectionBounds = new Bounds ( selectionCube.transform.position , new Vector3 (
                        Vector3.Distance(points[0] , points[1]) ,
                        (points[0].y-PlaceController.Instance.place.LowestY) ,
                        Vector3.Distance(points[1] , points[2]) 
                ));
            }
        }


        public Vector2Int TerrainPosition(Vector3 point)
        {
            return new Vector2Int(
                (int)((point.x / terrainData.size.x) * (float)terrainData.heightmapResolution) ,
                (int)((point.z / terrainData.size.z) * (float)terrainData.heightmapResolution)
            );
        }

        public Vector2 PositionTerrain(Vector2Int Tpoint)
        {
            return new Vector2(
                terrainData.size.x * (Tpoint.x / (float)terrainData.heightmapResolution) ,
                terrainData.size.z * (Tpoint.y / (float)terrainData.heightmapResolution)
            );
        }

        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) 
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }

        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation) 
        {
            return rotation * (point - pivot) + pivot;
        }

        public bool isInsideSelection(Vector3 point)
        {
            return selectionBounds.Contains(RotatePointAroundPivot(point , selectionCube.transform.position , Quaternion.Inverse(selectionCube.transform.rotation) ));
        }

        public bool isInsideSelection(Vector2 point)
        {
            Vector3 point3D = new Vector3(point.x , points[0].y , point.y);
            return isInsideSelection(point3D);
        }

        public void Flatten(Vector3 position, int width, int length){
            int xBase = (int)(position.x - terrain.GetPosition().x);
            int yBase = (int)(position.z - terrain.GetPosition().z);
     
            float height = terrainData.GetInterpolatedHeight((position.x - terrain.GetPosition().x)/terrainData.heightmapResolution, (position.z - terrain.GetPosition().z)/terrainData.heightmapResolution);
            float normalizedHeight = height / terrainData.size.y;
            float[,] heights = new float[width, length];
     
            for(int i=0; i<width; ++i)
                for(int j=0; j<length; ++j)
                    heights[i,j] = normalizedHeight;
     
           
            terrainData.SetHeights(xBase, yBase, heights);
        }

    }
}
