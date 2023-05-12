using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //Intersection, which stores the result of an intersection.
    //Apart from the intersection distance, you will at least want to store the nearest primitive, but perhaps also the normal at the intersection point.
    internal class Intersection
    {
        internal float distance;
        internal Primitive nearestPrimitive;
        internal Vector3 normal;
        internal Vector3 position;
        internal Intersection(float distance, Primitive nearestPrimitive, Vector3 normal, Vector3 position)
        {
            this.distance = distance;
            this.nearestPrimitive = nearestPrimitive;
            this.normal = normal;
            this.position = position;
        }
    }
}
