using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{

    public class Template
    {
        public string name { get; protected set; }
        public List<Feature> features { get; protected set; }
        public Vector3 castBox { get; protected set; }

        public GameObject prefab { get; protected set; }

        public Template(string n, List<Feature> lf, Vector3 cb , GameObject prefab_go)
        {
            name = n;
            features = lf;
            castBox = cb;
            prefab = prefab_go;
        }
    }
}
