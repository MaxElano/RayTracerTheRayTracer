using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //Primitive, which encapsulates the ray/primitive intersection functionality.
    //Two classes canbe derived from the base primitive: Sphere and Plane.
    //A sphere is defined by a positionand a radius; a plane is defined by a normal and a distanceto the origin.
    //Initially (until you implement materials) it may also be useful to add a color to the primitive class.
    internal class Primitive
    {
        
    }

    internal class Sphere : Primitive
    {
        Vector3 position;
        float radius;
    }

    internal class Plane : Primitive
    {
        Vector3 normal;
        float distanceToOrigin; //Could also be Vector3, but depends
    }
}
