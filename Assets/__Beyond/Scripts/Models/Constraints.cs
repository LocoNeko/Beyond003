using System.Collections.Generic;
using UnityEngine;
using System;

namespace Beyond
{
    public class Constraints
    {
        public string operation {get; protected set;}
        public List<Constraints> constraintsList {get; protected set;}
        public List<string> templateNamesList {get; protected set;}
        public List<Vector3Int> offsetsList {get; protected set;}
        public List<cellSide> cellSides {get; protected set;}
        public float depth {get; protected set;}
        public LayerMask mask { get; protected set; }

        public static Constraints Create_OR(List<Constraints> constraintsList)
        {
            Constraints c = new Constraints();
            c.operation = "OR";
            c.constraintsList = new List<Constraints>() ;
            foreach (Constraints c2 in constraintsList)
            {
                c.constraintsList.Add(c2);
            }
            return c ;
        }

        public static Constraints Create_AND(List<Constraints> constraintsList)
        {
            Constraints c = new Constraints();
            c.operation = "AND";
            c.constraintsList = new List<Constraints>() ;
            foreach (Constraints c2 in constraintsList)
            {
                c.constraintsList.Add(c2);
            }
            return c ;
        }

        public static Constraints Create_BASEIN(float depth)
        {
            Constraints c = new Constraints();
            c.operation = "BASEIN";
            c.depth = depth ;
            return c ;
        }

        public static Constraints Create_NEEDSONE(List<string> templateNames , List<Vector3Int> offsets, List<cellSide> sides)
        {
            Constraints c = new Constraints();
            c.operation = "NEEDSONE";
            c.templateNamesList = new List<string>();
            c.offsetsList = new List<Vector3Int>();
            c.cellSides = new List<cellSide>();

            foreach (string s in templateNames)
            {
                c.templateNamesList.Add(s);
            }
            foreach (Vector3Int v in offsets)
            {
                c.offsetsList.Add(v);
            }
            foreach (cellSide cs in sides)
            {
                c.cellSides.Add(cs);
            }
            return c ;
        }

        public static Constraints Create_NEEDSALL(List<string> templateNames , List<Vector3Int> offsets, List<cellSide> sides)
        {
            Constraints c = new Constraints();
            c.operation = "NEEDSALL";
            c.templateNamesList = new List<string>();
            c.offsetsList = new List<Vector3Int>();
            c.cellSides = new List<cellSide>();

            foreach (string s in templateNames)
            {
                c.templateNamesList.Add(s);
            }
            foreach (Vector3Int v in offsets)
            {
                c.offsetsList.Add(v);
            }
            foreach (cellSide cs in sides)
            {
                c.cellSides.Add(cs);
            }
            return c ;
        }

        public static Constraints Create_ALLCLEAR(LayerMask lm)
        {
            Constraints c = new Constraints();
            c.operation = "ALLCLEAR";
            c.mask = lm ;
            return c ;
        }

        public static Constraints Create_TOPCLEAR(LayerMask lm)
        {
            Constraints c = new Constraints();
            c.operation = "TOPCLEAR";
            c.mask = lm ;
            return c ;
        }

        public static Constraints Create_FIRSTINGROUP()
        {
            Constraints c = new Constraints();
            c.operation = "FIRSTINGROUP";
            return c ;
        }

        public static string ShowConstraints(Constraints c)
        {
            if (c.operation == "OR" || c.operation == "AND")
            {
                string listofconstraints = "" ;
                foreach (Constraints c2 in c.constraintsList)
                {
                    listofconstraints+=ShowConstraints(c2)+",";
                }
                return c.operation+"("+listofconstraints+")";
            }
            else return c.operation ;
        }

    }
}
