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
        internal enum Materials { vacuum, air, water, ice, plastic, window_glass, diamond };
        internal Vector3 speculalColor;
        internal Vector3 materialColor;
        internal float specularity;
        internal Vector3 specularColor;
        internal float opticalDensity;

        internal Primitive(Vector3 materialColor, Vector3 speculalColor, float specularity = 0, Materials material = Materials.vacuum)
        {
            this.materialColor = materialColor;
            this.speculalColor = speculalColor; //White or gray for plastics and same as base color for metals;
            this.specularity = specularity;
            this.specularColor = materialColor;
            opticalDensity = DetermineOpticalDensity(material);

        }
        
        internal static float DetermineOpticalDensity(Materials material)
        {
            switch (material)
            {
                case Materials.vacuum:
                    return 1;
                case Materials.air:
                    return 1.000293f;
                case Materials.water:
                    return 1.333f;
                case Materials.ice:
                    return 1.31f;
                case Materials.plastic:
                    return 1.49f;
                case Materials.window_glass:
                    return 1.52f;
                case Materials.diamond:
                    return 2.42f;
                default:
                    return 1;
            }
        }
    }

    internal class Sphere : Primitive
    {
        internal Vector3 position;
        internal float radius;
        internal Sphere(Vector3 position, float radius, Vector3 materialColor, Vector3 speculalColor, float specularity = 0, Materials material = Materials.vacuum) : base(materialColor, speculalColor, specularity, material)
        {
            this.position = position;
            this.radius = radius;
        }
    }

    internal class Plane : Primitive
    {
        internal Vector3 normal;
        internal Vector3 distanceToOrigin;
        internal Plane(Vector3 normal, Vector3 distanceToOrigin, Vector3 materialColor, Vector3 speculalColor, float specularity = 0, Materials material = Materials.vacuum) : base(materialColor, speculalColor, specularity, material)
        {
            this.normal = normal;
            normal.Normalize();
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
        internal Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 materialColor, Vector3 speculalColor, float specularity = 0, Materials material = Materials.vacuum) : base(materialColor, speculalColor, specularity, material)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.pointC = pointC;
            normal = -Vector3.Cross((pointB - pointA), (pointC - pointA)) / Math.Abs(Vector3.Cross((pointB - pointA), (pointC - pointA)).Length);
            normal.Normalize();
        }
    }

    internal class TexturedSphere : Sphere
    {
        internal TexturedSphere(Vector3 position, float radius, Vector3 materialColor, Vector3 specularColor, float specularity = 0, Materials material = Materials.vacuum) : base(position, radius, materialColor, specularColor, specularity, material)
        {

        }
    } 

    internal class TexturedTriangle : Triangle
    {
        internal TexturedTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 materialColor, Vector3 speculalColor, float specularity = 0, Materials material = Materials.vacuum) : base(pointA, pointB, pointC, materialColor, speculalColor, specularity, material)
        {

        }
    }

    internal class TexturedPlane : Plane
    {
        internal TexturedPlane(Vector3 normal, Vector3 distanceToOrigin, Vector3 materialColor, Vector3 speculalColor, float specularity = 0, Materials material = Materials.vacuum) : base(normal,distanceToOrigin,materialColor,speculalColor, specularity, material)
        {
            base.normal = normal;
            base.normal.Normalize();
            base.distanceToOrigin = distanceToOrigin;
        }
    }
} 
