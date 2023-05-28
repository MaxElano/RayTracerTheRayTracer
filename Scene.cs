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
        internal List<Primitive> primitives;
        internal List<Light> lights;
        internal Scene()
        {
            primitives = new List<Primitive>();
            lights = new List<Light>();

            //primitives.Add(new Plane(new Vector3(0,1,0), new Vector3(0, -4, 0), new Vector3(0.2f,0.2f,0.2f), new Vector3(0.2f,0.2f,0.2f)));
            primitives.Add(new Sphere(new Vector3(0, 0, 4), 1f, new Vector3(1, 0, 0), new Vector3(0.8f, 0.8f, 0.8f)));
            primitives.Add(new Sphere(new Vector3(2.5f, 0, 4), 1f, new Vector3(0, 1, 0), new Vector3(0.8f, 0.8f, 0.8f)));
            primitives.Add(new Sphere(new Vector3(0, 2.5f, 4), 1f, new Vector3(0, 0, 1), new Vector3(0.8f, 0.8f, 0.8f)));
            
            Sphere mirror = new Sphere(new Vector3(2.5f, 2.5f, 4), 1f, new Vector3(1, 1, 1), new Vector3(0.8f, 0.8f, 0.8f), 0.1f);
            mirror.specularColor = new Vector3(0.7f, 0.7f, 0.7f);
            primitives.Add(mirror);
            
            lights.Add(new Light(new Vector3(1.25f, 1.25f, -1), new Vector3(10, 10, 10)));
        }

        internal Intersection PrimaryRayIntersection(Ray ray)
        {
            float distance = 0;
            Primitive nearestPrimitive = null;
            Vector3 normal = Vector3.Zero;
            Vector3 pointOfIntersection = Vector3.Zero;
            int primitivesCount = primitives.Count;
            for (int i = 0; i < primitivesCount; i++)
            {
                Primitive primitive = primitives[i];
                Intersection tempIntersection = null;
                if (primitives[i] is Plane)
                {
                    tempIntersection = collideRayPlane(ray, primitive as Plane);
                }
                else if (primitives[i] is Sphere)
                {
                    tempIntersection = collideRaySphere(ray, primitive as Sphere);
                }
                
                if(tempIntersection.distance > Application.epsilon && (tempIntersection.distance < distance - Application.epsilon || distance == 0))
                {
                    distance = tempIntersection.distance;
                    nearestPrimitive = primitive;
                    normal = tempIntersection.normal;
                    pointOfIntersection = tempIntersection.position;
                }
            }
            Intersection intersection = new Intersection(distance, nearestPrimitive, normal, pointOfIntersection);

            return intersection;
        }
        

        internal Vector3 FindShadowRayColor(Ray ray, Light light)
        {
            int primitivesCount = primitives.Count;
            Intersection tempIntersection = null;
            for (int i = 0; i < primitivesCount; i++)
            {
                if (primitives[i] is Plane)
                {
                    tempIntersection = collideRayPlane(ray, primitives[i] as Plane);
                }
                else if (primitives[i] is Sphere)
                {
                    tempIntersection = collideRaySphere(ray, primitives[i] as Sphere);
                }
                if (tempIntersection.distance > Application.epsilon && tempIntersection.distance < ray.intersectionDistance - Application.epsilon)
                {
                    return Vector3.Zero;
                }
            }
            return light.rgbIntensity;
        }

        internal Intersection collideRayPlane(Ray ray, Plane primitive)
        {
            Vector3 pointOfIntersection = Vector3.Zero;
            Vector3 tempNormal = Vector3.Zero;
            float length = 0;
            if (Vector3.Dot(ray.direction, primitive.normal) != 0) //If the ray and plane are not parallel
            {
                pointOfIntersection = ray.origin - ray.direction * (Vector3.Dot(ray.origin - primitive.distanceToOrigin, primitive.normal) / Vector3.Dot(ray.direction, primitive.normal));
                tempNormal = primitive.normal;
                tempNormal.Normalize();
                Vector3 direction = pointOfIntersection - ray.origin;
                direction.Normalize();
                if (direction == ray.direction)
                    length = (pointOfIntersection - ray.origin).Length;
            }
            return new Intersection(length, primitive, tempNormal, pointOfIntersection);
        }

        internal Intersection collideRaySphere(Ray ray, Sphere primitive)
        {
            float length = 0;
            Vector3 pointOfIntersection = Vector3.Zero;
            Vector3 tempNormal = Vector3.Zero;
            float a = ray.direction.X * ray.direction.X + ray.direction.Y * ray.direction.Y + ray.direction.Z * ray.direction.Z;
            float b = (2 * (ray.origin.X - primitive.position.X) * ray.direction.X) + (2 * (ray.origin.Y - primitive.position.Y) * ray.direction.Y) + (2 * (ray.origin.Z - primitive.position.Z) * ray.direction.Z);
            float c = (ray.origin.X - primitive.position.X) * (ray.origin.X - primitive.position.X) + (ray.origin.Y - primitive.position.Y) * (ray.origin.Y - primitive.position.Y) + (ray.origin.Z - primitive.position.Z) * (ray.origin.Z - primitive.position.Z) - primitive.radius * primitive.radius;

            float t = 0;
            float d = b * b - 4 * a * c;
            if (d >= 0 && a >= 0) //If there are solutions
            {
                t = (float)((-b - Math.Sqrt(d)) / (2 * a));
                if (t > 0)
                {
                    length = (ray.direction * t).Length;
                    pointOfIntersection = ray.origin + ray.direction * t;
                    tempNormal = pointOfIntersection - primitive.position;
                    tempNormal.Normalize();
                }
            }
            return new Intersection(length, primitive, tempNormal, pointOfIntersection);
        }
    }
}
