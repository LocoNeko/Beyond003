using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public static class Utility
    {
        public static Vector3 RotateAroundPoint(Vector3 p, Vector3 pivot, Quaternion r)
        {
            return r * (p - pivot) + pivot;
        }

        public static Vector3 ClosestPointOnLine(Vector3 a, Vector3 b, Vector3 p)
        {
            return a + Vector3.Project(p - a, b - a);
        }

        // Just a debug thing
        public static void DrawPlane(Vector3 position, Vector3 normal)
        {
            Vector3 v3;

            if (normal.normalized != Vector3.forward)
                v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
            else
                v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

            v3 *= 5;

            var corner0 = position + v3;
            var corner2 = position - v3;
            var q = Quaternion.AngleAxis(90f, normal);
            v3 = q * v3;
            var corner1 = position + v3;
            var corner3 = position - v3;

            Debug.DrawLine(corner0, corner2, Color.green, 1f);
            Debug.DrawLine(corner1, corner3, Color.green, 1f);
            Debug.DrawLine(corner0, corner1, Color.green, 1f);
            Debug.DrawLine(corner1, corner2, Color.green, 1f);
            Debug.DrawLine(corner2, corner3, Color.green, 1f);
            Debug.DrawLine(corner3, corner0, Color.green, 1f);
            Debug.DrawRay(position, normal, Color.red, 1f);
        }

    }
}