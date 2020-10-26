using System.Collections.Generic;
using UnityEngine;
using System;

namespace Beyond
{

    // This script is attached to all objects specific to Beyond that can be created & placed
    public class Constraints : MonoBehaviour
    {
        string operation;
        List<Constraints> constraintsList;
        List<Template> templatesList; // I could pass template names (strings) instead of templates
        List<Vector3Int> offsetsList;
        List<int> cellSides;
        float depth;

        public Constraints(string op, List<Constraints> cl , List<Template> tl , List<Vector3Int> ol, List<int> csl ,float d)
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
                case "TOPCLEAR":
                case "ALLCLEAR":
                default:
                    return;
            }
        }

        public bool CheckConstraints(BeyondComponent bc)
        {
            switch(operation)
            {
                case "OR":
                    foreach (Constraints c in constraintsList)
                    {
                        if (c.CheckConstraints(bc)) return true;
                    }
                    return false;
                case "AND":
                    foreach (Constraints c in constraintsList)
                    {
                        if (!c.CheckConstraints(bc)) return false;
                    }
                    return true;
                case "TOPCLEAR":
                    return TopClear(bc);
                case "BASEIN":
                    return BaseInput(bc , depth);
                case "ALLCLEAR":
                    return AllClear(bc);
                case "NEEDSONE":
                    return NeedsOne(bc , templatesList, offsetsList, cellSides);
                case "NEEDSALL":
                    return NeedsAll(bc , templatesList, offsetsList, cellSides);
                default:
                    return false;
            }
        }

        private static bool TopClear(BeyondComponent bc)
        {
            throw new NotImplementedException();
        }
        private static bool BaseInput(BeyondComponent bc , float depth)
        {
            throw new NotImplementedException();
        }

        private static bool AllClear(BeyondComponent bc)
        {
            throw new NotImplementedException();
        }

        private static bool NeedsOne(BeyondComponent bc, List<Template> templatesList, List<Vector3Int> offsetsList, List<int> cellSides)
        {
            throw new NotImplementedException();
        }

        private static bool NeedsAll(BeyondComponent bc, List<Template> templatesList, List<Vector3Int> offsetsList, List<int> cellSides)
        {
            throw new NotImplementedException();
        }
    }
}
