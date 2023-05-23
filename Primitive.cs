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
        internal float specularity;
        internal Vector3 specularColor;

        internal Primitive(Vector3 materialColor, Vector3 speculalColor, float specularity)
        {
            this.materialColor = materialColor;
            this.speculalColor = speculalColor; //White or gray for plastics and same as base color for metals;
            this.specularity = specularity;
            this.specularColor = materialColor;
        }
    }

    internal class Sphere : Primitive
    {
        internal Vector3 position;
        internal float radius;
        internal Sphere(Vector3 position, float radius, Vector3 materialColor, Vector3 speculalColor, float specularity = 0) : base(materialColor, speculalColor, specularity)
        {
            this.position = position;
            this.radius = radius;
        }
    }

    internal class Plane : Primitive
    {
        internal Vector3 normal;
        internal Vector3 distanceToOrigin;
        internal Plane(Vector3 normal, Vector3 distanceToOrigin, Vector3 materialColor, Vector3 speculalColor, float specularity = 0) : base(materialColor, speculalColor, specularity)
        {
            this.normal = normal;
            this.distanceToOrigin = distanceToOrigin;
        }
    }

    internal class Triangle : Primitive
    {
        internal Vector3 pointA;
        internal Vector3 pointB;
        internal Vector3 pointC;
        internal Vector3 normal;
        internal float alpha;
        internal float beta;
        internal float gamma;
        internal Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 materialColor, Vector3 speculalColor, float specularity = 0) : base(materialColor, speculalColor, specularity)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.pointC = pointC;
            normal = Vector3.Cross((pointB - pointA), (pointC - pointA)) / Math.Abs(Vector3.Cross((pointB - pointA), (pointC - pointA)).Length);
            normal.Normalize();
        }
    }
}
