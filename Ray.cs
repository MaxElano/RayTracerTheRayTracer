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
        internal int numberOfBounces;
        internal float opticalDensity;
        internal Ray(Vector3 origin, Vector3 direction, float intersectionDistance = 0, int numberOfBounces = 0, float opticalDensity = 0, Primitive.Materials material = Primitive.Materials.air)
        {
            this.origin = origin;
            this.direction = direction;
            this.intersectionDistance = intersectionDistance;
            this.numberOfBounces = numberOfBounces;
            this.opticalDensity = opticalDensity;
            if(opticalDensity == 0)
                this.opticalDensity = Primitive.DetermineOpticalDensity(material);

        }
    }
}
