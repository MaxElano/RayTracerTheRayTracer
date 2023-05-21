using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    internal class Ray
    {
        internal Vector3 origin;
        internal Vector3 direction;
        internal float intersectionDistance;
        internal Ray(Vector3 origin, Vector3 direction, float intersectionDistance = 0)
        {
            this.origin = origin;
            this.direction = direction;
            this.intersectionDistance = intersectionDistance;
        }
    }
}
