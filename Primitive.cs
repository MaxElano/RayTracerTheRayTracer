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
        internal Vector3 speculalColor;
        internal Vector3 materialColor;

        internal Primitive(Vector3 materialColor, Vector3 speculalColor)
        {
            this.materialColor = materialColor;
            this.speculalColor = speculalColor; //White or gray for plastics and same as base color for metals;
        }
    }

    internal class Sphere : Primitive
    {
        internal Vector3 position;
        internal float radius;
        internal Sphere(Vector3 position, float radius, Vector3 materialColor, Vector3 speculalColor) : base(materialColor, speculalColor)
        {
            this.position = position;
            this.radius = radius;
        }
    }

    internal class Plane : Primitive
    {
        internal Vector3 normal;
        internal Vector3 distanceToOrigin;
        internal Plane(Vector3 normal, Vector3 distanceToOrigin, Vector3 materialColor, Vector3 speculalColor) : base(materialColor, speculalColor)
        {
            this.normal = normal;
            this.distanceToOrigin = distanceToOrigin;
        }
    }
}
