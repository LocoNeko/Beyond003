using System.Collections.Generic;
using UnityEngine;
using System;

namespace Beyond
{

    // This script is attached to all objects specific to Beyond that can be created & placed
    public class Constraints : MonoBehaviour
    {
        public string operation {get; protected set;}
        public List<Constraints> constraintsList {get; protected set;}
        public List<Template> templatesList {get; protected set;} // I could pass template names (strings) instead of templates
        public List<Vector3Int> offsetsList {get; protected set;}
        public List<int> cellSides {get; protected set;}
        public float depth {get; protected set;}
        public LayerMask mask { get; protected set; }

        public Constraints()
        {

        }

        //TODO : nope. Make static methods for each type of constraint
        public Constraints(string op, List<Constraints> cl , List<Template> tl , List<Vector3Int> ol, List<int> csl ,float d , LayerMask lm)
        {
            operation = op;
            switch (operation)
            {
                case "OR":
                    constraintsList = new List<Constraints>();
                    foreach (Constraints c in cl)
                    {
                        constraintsList.Add(c);
                    }
                    break;
                case "AND":
                    constraintsList = new List<Constraints>();
                    foreach (Constraints c in cl)
                    {
                        constraintsList.Add(c);
                    }
                    break;
                case "BASEIN":
                    depth = d;
                    break;
                case "NEEDSONE":
                case "NEEDSALL":
                    templatesList = new List<Template>();
                    offsetsList = new List<Vector3Int>();
                    cellSides = new List<int>();
                    foreach (Template t in tl)
                    {
                        templatesList.Add(t);
                    }
                    foreach (Vector3Int v in ol)
                    {
                        offsetsList.Add(v);
                    }
                    foreach (int cs in csl)
                    {
                        cellSides.Add(cs);
                    }
                    break;
                case "ALLCLEAR":
                    mask = lm;
                    break;
                case "TOPCLEAR":
                default:
                    return;
            }
        }

        public static Constraints Create_OR(List<Constraints> cl)
        {
            Constraints c = new Constraints();
            c.operation = "OR";
            c.constraintsList = new List<Constraints>() ;
            foreach (Constraints c2 in cl)
            {
                c.constraintsList.Add(c2);
            }
            return c ;
        }

        public static Constraints Create_AND(List<Constraints> cl)
        {
            Constraints c = new Constraints();
            c.operation = "AND";
            c.constraintsList = new List<Constraints>() ;
            foreach (Constraints c2 in cl)
            {
                c.constraintsList.Add(c2);
            }
            return c ;
        }
    }
}
