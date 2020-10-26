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
        public GameObject prefab { get; protected set; }

        public Template(string name, Vector3 castBox , GameObject prefab_go , Vector3? pivotOffset=null)
        {
            this.name = name;
            this.castBox = castBox;
            this.prefab = prefab_go;
            if (pivotOffset!=null)
            {
                this.pivotOffset = (Vector3)pivotOffset;
            }
            else
            {
                this.pivotOffset = Vector3.zero;
            }
        }
    }
}
