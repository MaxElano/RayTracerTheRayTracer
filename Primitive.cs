using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //Primitive, which encapsulates the ray/primitive intersection functionality.
    //Two classes can be derived from the base primitive: Sphere and Plane.
    //A sphere is defined by a position and a radius; a plane is defined by a normal and a distance to the origin.
    //Initially (until you implement materials) it may also be useful to add a color to the primitive class.
    internal class Primitive
    {
        float redIntensity; //Temp
        float greenIntensity; //Temp
        float blueIntensity; //Temp
        internal Primitive(float redIntensity, float greenIntensity, float blueIntensity)
        {
            this.redIntensity = redIntensity;
            this.greenIntensity = greenIntensity;
            this.blueIntensity = blueIntensity;
        }
    }

    internal class Sphere : Primitive
    {
        Vector3 position;
        float radius;
        internal Sphere(Vector3 position, float radius, float redIntensity, float greenIntensity, float blueIntensity) : base(redIntensity, greenIntensity, blueIntensity)
        {
            this.position = position;
            this.radius = radius;
        }
    }

    internal class Plane : Primitive
    {
        Vector3 normal;
        float distanceToOrigin; //Could also be Vector3, but depends
        internal Plane(Vector3 normal, float distanceToOrigin, float redIntensity, float greenIntensity, float blueIntensity) : base(redIntensity, greenIntensity, blueIntensity)
        {
            this.normal = normal;
            this.distanceToOrigin = distanceToOrigin;
        }
    }
}
