using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //Scene, which stores a list of primitives and light sources.
    //It implements a scene-level Intersect method, which loops over the primitives and returns the closest intersection.
    internal class Scene
    {
        List<Primitive> primitives;
        List<Light> lights;
        internal Scene()
        {
            primitives = new List<Primitive>();
            lights = new List<Light>();

            //primitives.Add(new Plane(new Vector3(1,0,0), new Vector3(0, 0, -5), new Vector3(1,0,0)));
            primitives.Add(new Sphere(new Vector3(0, 0, 5), 1f, new Vector3(1, 0, 0)));
            //primitives.Add(new Sphere(new Vector3(0, 0.5f, 3), 1f, new Vector3(0, 1, 0)));
        }
        internal Intersection IntersectClosestObject(Ray ray)
        {
            float distance = 0;
            Primitive nearestPrimitive = null;
            Vector3 normal = Vector3.Zero;
            for (int i = 0; i < primitives.Count; i++)
            {
                float length = 0;
                Vector3 pointOfIntersection = Vector3.Zero;
                Vector3 tempNormal = Vector3.Zero;
                Primitive primitive = primitives[i];
                if (primitives[i] is Plane)
                {
                    if (Vector3.Dot(ray.direction, (primitive as Plane).normal) != 0) //If the ray and plane are not parallel
                    {
                        pointOfIntersection = ray.origin - ray.direction * (Vector3.Dot(ray.origin - (primitive as Plane).distanceToOrigin, (primitive as Plane).normal) / Vector3.Dot(ray.direction, (primitive as Plane).normal));
                        tempNormal = (primitive as Plane).normal;
                        tempNormal.Normalize();
                        length = (pointOfIntersection - ray.origin).Length;
                    }
                }
                else if (primitives[i] is Sphere)
                {
                    float a = ray.direction.X * ray.direction.X + ray.direction.Y * ray.direction.Y + ray.direction.Z * ray.direction.Z;
                    float b = (2 * (ray.origin.X - (primitive as Sphere).position.X) * ray.direction.X) + (2 * (ray.origin.Y - (primitive as Sphere).position.Y) * ray.direction.Y) + (2 * (ray.origin.Z - (primitive as Sphere).position.Z) * ray.direction.Z);
                    float c = (ray.origin.X - (primitive as Sphere).position.X) * (ray.origin.X - (primitive as Sphere).position.X) + (ray.origin.Y - (primitive as Sphere).position.Y) * (ray.origin.Y - (primitive as Sphere).position.Y) + (ray.origin.Z - (primitive as Sphere).position.Z) * (ray.origin.Z - (primitive as Sphere).position.Z) - (primitive as Sphere).radius * (primitive as Sphere).radius;

                    float t = 0;
                    float d = b * b - 4 * a * c;
                    if (d >= 0 && a >= 0) //If there are solutions
                    {
                        t = (float)((-b - Math.Sqrt(d)) / (2 * a));
                        length = (ray.direction * t).Length;
                        pointOfIntersection = ray.origin + ray.direction * t;
                        tempNormal = (pointOfIntersection - (primitive as Sphere).position);
                        tempNormal.Normalize();
                    }
                }
                
                if(length > 0 && (length < distance || distance == 0))
                {
                    distance = length;
                    nearestPrimitive = primitive;
                    normal = tempNormal;
                }
            }
            Intersection intersection = new Intersection(distance, nearestPrimitive, normal);

            return intersection;
        }
    }
}
