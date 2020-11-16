using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    [Serializable]
    public class Template
    {
        public string name { get; protected set; }
        // The cast box allows us to prevent the object from being inside terrain and is a good alternative to colliders
        public Vector3 castBox { get; protected set; }
        //The pivotOffset tells us where the cell's centre is compared to the object's centre
        public Vector3 pivotOffset { get; protected set; }
        public List<Vector3Int> cells { get; protected set; }
        public GameObject prefab { get; protected set; }
        public Constraints constraints { get; protected set; }
        public int dragDimensions { get; protected set; }
        public cellSide? fixedSide { get; protected set; }

        public Template(string name, Vector3 castBox , GameObject prefab_go , Constraints constraints , int dragDimensions, cellSide? fixedSide = null, Vector3? pivotOffset=null , List<Vector3Int> cells=null)
        {
            this.name = name;
            this.castBox = castBox;
            if (cells!=null)
            {
                this.cells = new List<Vector3Int>() ;
                foreach (Vector3Int v in cells)
                {
                    this.cells.Add(v) ;
                }
            }
            this.prefab = prefab_go;
            this.constraints = constraints ;
            this.dragDimensions = dragDimensions ;
            this.fixedSide = fixedSide ;

            if (pivotOffset!=null)
                this.pivotOffset = (Vector3)pivotOffset;
            else
                this.pivotOffset = Vector3.zero;
        }
    }
}
