using Microsoft.VisualBasic;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenTK.Graphics.OpenGL.GL;

namespace RayTracer
{
    //Raytracer, which owns the scene, camera and the display surface.
    //The Raytracer implements a method Render, which uses the camera to loop over the pixels of the screen plane and to generate a ray for each pixel,
    //which is then used to find the nearest intersection. The result is then visualized by plotting a pixel.
    //For the middle row of pixels (typically line 256 for a 512x512 window), it generates debug output by visualizing every Nthray (where N is e.g. 10).
    internal class Raytracer
    {
        internal Scene scene;
        internal Camera camera;
        Surface screen, map, background;

        int tempint = 0;
        Ray primaryRay;
        Intersection primaryIntersection;

        int numberOfBouncesAllowed = 5;

        Vector3 ambientLightRadiance = new Vector3(0.05f, 0.05f, 0.05f);

        bool showBackground = true;

        //-------Debugging------
        bool showPrimaryRays = true;
        bool showShadowRays = true;
        bool showReflectionRays = true;
        bool showRefractionRays = true;
        bool showPrimitives = true;

        //-------Multi Threading------
        internal static object lockObject = new object();
        internal bool debugMode, multiThreading = true;

        internal Raytracer(Surface screen)
        {
            scene = new Scene();
            camera = new Camera(new Vector3(0,0,0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 1f, screen);
            debugMode = false;
            this.screen = screen;
            map = new Surface("../../../assets/sus.png");
            background = new Surface("../../../assets/skybox.png");
        }
        internal void Render()
        {
            if (debugMode)
            {
                screen.Clear(0x000000);

                screen.Line(TX(camera.leftBottom.X), TY(-camera.position.Z - camera.distanceToScreenPlane), TX(camera.rightBottom.X), TY(-camera.position.Z - camera.distanceToScreenPlane), 0xffffff);
                screen.Box(TX(camera.position.X) - 1, TY(-camera.position.Z) + 1, TX(camera.position.X) + 1, TY(-camera.position.Z) - 1, 0xffffff);

                if (showPrimitives)
                {
                    foreach (var item in scene.primitives)
                        if (item is Sphere)
                        {
                            Sphere sphere = (Sphere)item;

                            for (float j = 0; j < 360; j += 3.6f)
                            {
                                float tempX1 = sphere.radius * (float)Math.Cos(MathHelper.DegreesToRadians(j));
                                float tempX2 = sphere.radius * (float)Math.Cos(MathHelper.DegreesToRadians(j + 3.6f));
                                float tempY1 = sphere.radius * (float)Math.Sin(MathHelper.DegreesToRadians(j));
                                float tempY2 = sphere.radius * (float)Math.Sin(MathHelper.DegreesToRadians(j + 3.6f));

                                screen.Line(TX(tempX1 + sphere.position.X), TY(tempY1 - sphere.position.Z), TX(tempX2 + sphere.position.X), TY(tempY2 - sphere.position.Z), 0xffffff);
                            }
                        }
                }

                for (int x = 0; x < screen.width; x++)
                    if (x % 1 == 0 || x == 0)
                    {
                        primaryRay = FindPrimaryRay(x, screen.height / 2, screen.width, screen.height);
                        DebugTrace(primaryRay, scene, 0xfcba03);
                    }
            }
            else
            {
                if (multiThreading)
                {
                    Parallel.For(0, screen.height, y =>
                    {
                        Parallel.For(0, screen.width, x =>
                        {
                            screen.pixels[x + y * screen.width] = 0;
                            Ray primaryRay = FindPrimaryRay(x, y, screen.width, screen.height);
                            Intersection primaryIntersection = PrimaryRayIntersection(primaryRay, scene);
                            Vector3 color;

                            if (primaryIntersection != null)
                            {
                                color = Trace(primaryRay, scene);
                                int tempColor = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256 + ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256 + (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                                screen.pixels[x + y * screen.width] = tempColor;
                            }
                            else
                            {
                                if (showBackground)
                                {
                                    color = Background(primaryRay.direction);
                                    int tempColor = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256 + ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256 + (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                                    screen.pixels[x + y * screen.width] = tempColor;
                                }
                                else
                                    screen.pixels[x + y * screen.width] = 0;
                            }
                        });
                    });
                }
                else
                {
                    for (int y = 0; y < screen.height; y++)
                    {
                        for (int x = 0; x < screen.width; x++)
                        {
                            screen.pixels[x + y * screen.width] = 0;
                            primaryRay = FindPrimaryRay(x, y, screen.width, screen.height);
                            primaryIntersection = PrimaryRayIntersection(primaryRay, scene);

                            if (primaryIntersection != null)
                            {
                                Vector3 color = Trace(primaryRay, scene);
                                int tempColor = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256 + ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256 + (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                                screen.pixels[x + y * screen.width] = tempColor;
                            }
                            else
                            {
                                if (showBackground)
                                {
                                    Vector3 color = Background(primaryRay.direction);
                                    int tempColor = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256 + ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256 + (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                                    screen.pixels[x + y * screen.width] = tempColor;
                                }
                                else
                                    screen.pixels[x + y * screen.width] = 0;
                            }
                        }
                    }
                }
            }
        }
        
        internal void DebugTrace(Ray ray, Scene scene, int nextColor)
        {
            Intersection intersection = PrimaryRayIntersection(ray, scene);
            Ray newRay;
            bool draw = false;

            switch (nextColor)
            {
                case (0xfcba03): //Primary Rays
                    if (showPrimaryRays)
                        draw = true;
                    break;
                case (0xff1100 or 0xc4b07b): //Shadow Rays and Failed Shadow Rays
                    if (showShadowRays)
                        draw = true;
                    break;
                case (0x8250c4): //Reflection Rays
                    if (showReflectionRays)
                        draw = true;
                    break;
                case (0x1aaaaa): //Refraction Rays
                    if (showRefractionRays)
                        draw = true;
                    break;
                default:
                    draw = true;
                    break;
            }

            if (intersection != null)
            {
                if(draw)
                    screen.Line(TX(ray.origin.X), TY(-ray.origin.Z), TX(intersection.position.X), TY(-intersection.position.Z), nextColor);

                //Pure Specular
                if (intersection.nearestPrimitive.specularity == 1)
                {
                    if (ray.numberOfBounces < numberOfBouncesAllowed)
                    {
                        Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                        reflectedVector.Normalize();
                        newRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);

                        DebugTrace(newRay, scene, 0x8250C4);
                    }
                }
                //----Refraction----
                else if (intersection.nearestPrimitive.opticalDensity != scene.sceneOpticalDensity && intersection.nearestPrimitive.opticalDensity != 1)
                {
                    Ray refractedRay = FindRefractionRay(intersection, ray);

                    if (refractedRay is not null && refractedRay.numberOfBounces < numberOfBouncesAllowed)
                    {
                        Vector3 normal = intersection.normal;
                        if (Vector3.Dot(normal, ray.direction) < 0)
                        {
                            normal *= -1;
                        }
                        screen.Line(TX(intersection.position.X), TY(-intersection.position.Z), TX(intersection.position.X + normal.X * 0.5f), TY(-(intersection.position.Z + normal.Z * 0.5f)), 0x326633);

                        Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, normal) * normal;
                        reflectedVector.Normalize();
                        Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);

                        DebugTrace(refractedRay, scene, 0x1AAAAA);
                        DebugTrace(reflectedRay, scene, 0x8250C4);
                    }
                }
                else //Phong Shading Model
                {
                    int lightsCount = scene.lights.Count();
                    Light light;
                    Ray shadowRay;
                    Intersection shadowIntersection;

                    //Partly Specular Reflections
                    if (intersection.nearestPrimitive.specularity != 0 && ray.numberOfBounces < numberOfBouncesAllowed)
                    {
                        Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                        reflectedVector.Normalize();
                        newRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);

                        DebugTrace(newRay, scene, 0x8250C4);
                    }

                    //Shadow Rays
                    for (int i = 0; i < lightsCount; i++)
                    {
                        light = scene.lights[i];
                        shadowRay = FindShadowRay(intersection, light);
                        shadowIntersection = PrimaryRayIntersection(shadowRay, scene);

                        if (shadowIntersection == null)
                        {
                            if (light is Spotlight && !(light as Spotlight).DoesItHit(shadowRay))
                            {
                                screen.Line(TX(intersection.position.X), TY(-intersection.position.Z), TX(light.position.X), TY(-light.position.Z), 0xc4b07b);
                            }
                            else
                            {
                                screen.Line(TX(intersection.position.X), TY(-intersection.position.Z), TX(light.position.X), TY(-light.position.Z), 0xff1100);
                            }
                        }
                        else
                        {
                            screen.Line(TX(intersection.position.X), TY(-intersection.position.Z), TX(shadowIntersection.position.X), TY(-shadowIntersection.position.Z), 0xc4b07b);
                        }

                    }
                }
            }
            else
            {
                if(draw)
                    screen.Line(TX(ray.origin.X), TY(-ray.origin.Z), TX(ray.origin.X + ray.direction.X * 80), TY(-(ray.origin.Z + ray.direction.Z * 80)), nextColor);
            }
        }

        internal Vector3 Trace(Ray ray, Scene scene) 
        {
            Vector3 color = Vector3.Zero;

            //-------------Start Searching For Closest Primitive-------------
            Intersection intersection = PrimaryRayIntersection(ray, scene);
            
            //-------------Ray Hit Something So Do...-------------
            if (intersection != null)
            {
                ray.intersectionDistance = Math.Abs((intersection.position - ray.origin).Length);
                //----Textures----
                Vector3 materialColor = intersection.nearestPrimitive.materialColor;
                if (intersection.nearestPrimitive is TexturedSphere)
                {
                    var sphere = (TexturedSphere)intersection.nearestPrimitive;

                    double theta = Math.Acos((intersection.position.Z - sphere.position.Z) / sphere.radius);
                    double phi = Math.Atan2(intersection.position.Y - sphere.position.Y, intersection.position.X - sphere.position.X);
                    double u = (phi + Math.PI) / (2 * Math.PI);
                    double v = theta / Math.PI;

                    materialColor = CheckboardPattern(u, v, 32);
                }
                else if (intersection.nearestPrimitive is TexturedTriangle)
                {
                    var triangle = (TexturedTriangle)intersection.nearestPrimitive;
                    double uA = 0;
                    double uB = 1;
                    double uC = 0.5;
                    double vA = 0;
                    double vB = 0;
                    double vC = 1;


                    double uP = ((TriangleIntersection)intersection).alpha * uA + ((TriangleIntersection)intersection).beta * uB + ((TriangleIntersection)intersection).gamma * uC;
                    double vP = ((TriangleIntersection)intersection).alpha * vA + ((TriangleIntersection)intersection).beta * vB + ((TriangleIntersection)intersection).gamma * vC;

                    materialColor = CheckboardPattern(uP, vP, 32);
                }
                else if (intersection.nearestPrimitive is TexturedPlane)
                {
                    var plane = (TexturedPlane)intersection.nearestPrimitive;
                    Vector3 n = new Vector3(1, 0, 0);
                    Vector3 vectorU = n - (Vector3.Dot(n, plane.normal) * plane.normal);
                    vectorU = Vector3.Normalize(vectorU);
                    Vector3 vectorV = Vector3.Cross(plane.normal, vectorU);

                    float u = Vector3.Dot(intersection.position - plane.distanceToOrigin, vectorU);
                    float v = Vector3.Dot(intersection.position - plane.distanceToOrigin, vectorV);

                    materialColor = CheckboardPattern(u, v, 4);
                }

                Vector3 materialsAmbientColor = materialColor;

                //----Pure Specular----
                if (intersection.nearestPrimitive.specularity == 1)
                {
                    if (ray.numberOfBounces < numberOfBouncesAllowed)
                    {
                        Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                        reflectedVector.Normalize();
                        Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);
                        color += materialsAmbientColor * ambientLightRadiance + materialColor * Trace(reflectedRay, scene) ;
                        return color;
                    }
                }
                //----Refraction----
                else if (intersection.nearestPrimitive.opticalDensity != scene.sceneOpticalDensity && intersection.nearestPrimitive.opticalDensity != 1)
                {
                    Ray refractedRay = FindRefractionRay(intersection, ray);
                    float refractedR = 1;
                    if (refractedRay is not null && refractedRay.numberOfBounces < numberOfBouncesAllowed)
                    {
                        Vector3 normal = intersection.normal;
                        if (Vector3.Dot(normal, ray.direction) < 0)
                        {
                            normal *= -1;
                        }
                        float pOD = ray.opticalDensity; //Previous Optical Density
                        float nOD = refractedRay.opticalDensity; //Next Optical Density
                        float tempR = ((nOD - pOD) / (nOD + pOD)) * ((nOD - pOD) / (nOD + pOD));
                        refractedR = tempR + (1 - tempR) * (float)(Math.Pow(1 - Vector3.Dot(ray.direction, normal), 5));

                        Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, normal) * normal;
                        reflectedVector.Normalize();
                        Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);

                        color += refractedR * Trace(reflectedRay, scene);
                        color += (1 - refractedR) * Trace(refractedRay, scene);
                        return color;
                    }
                }
                else //Phong Shading Model
                {
                    int lightsCount = scene.lights.Count();
                    Light light;
                    Ray shadowRay;
                    Vector3 intensity;
                    float distance;
                    Vector3 r;
                    float n;
                    Vector3 tempColor = Vector3.Zero;
                    bool partlySpecular = false;

                    if (intersection.nearestPrimitive.specularity != 0 && ray.numberOfBounces < numberOfBouncesAllowed) //Partly specular
                    {
                        Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                        reflectedVector.Normalize();
                        Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);
                        tempColor = intersection.nearestPrimitive.specularColor * Trace(reflectedRay, scene);
                        partlySpecular = true;
                    }

                    for (int i = 0; i < lightsCount; i++)
                    {
                        light = scene.lights[i];
                        shadowRay = FindShadowRay(intersection, light);
                        bool hits;
                        intensity = FindShadowRayColor(shadowRay, light, scene);
                        if (light is Spotlight && intensity != Vector3.Zero)
                        {
                            if (!(light as Spotlight).DoesItHit(shadowRay))
                                intensity = Vector3.Zero;
                            else
                            {

                            }
                        }

                        distance = shadowRay.intersectionDistance;

                        if (partlySpecular) //Partly specular
                        {
                            color += intensity * (1 / (distance * distance)) * materialColor * Math.Max(0, Vector3.Dot(intersection.normal, shadowRay.direction)) + tempColor;
                        }
                        else if (intensity != Vector3.Zero)
                        {
                            n = 20;
                            r = -shadowRay.direction - 2 * Vector3.Dot(-shadowRay.direction, intersection.normal) * intersection.normal;
                            r.Normalize();
                            color += intensity * (1 / (distance * distance)) * (materialColor * Math.Max(0, Vector3.Dot(intersection.normal, shadowRay.direction)) + intersection.nearestPrimitive.speculalColor * (float)Math.Pow(Math.Max(0, Vector3.Dot(-ray.direction, r)), n));
                        }
                    }

                    

                    color += materialsAmbientColor * ambientLightRadiance;
                }
                


                //if (intersection.nearestPrimitive is Triangle)
                //{
                //    color = new Vector3(color.X * ((TriangleIntersection)intersection).alpha, color.Y * ((TriangleIntersection)intersection).beta, color.Z * ((TriangleIntersection)intersection).gamma);
                //}
                //

                return color;
            }
            else
            {
                if(showBackground)
                    return Background(ray.direction);
                else
                    return Vector3.Zero;
            }
        }

        internal Vector3 CheckboardPattern(double u, double v, int factor)
        {
            int opacity = (int)(u*factor) + (int)(v*factor) & 1;
            Vector3 finalColor = new Vector3(1f, 1f, 1f);

            return opacity * finalColor;
        }

        internal Vector3 Sus(double u, double v)
        {
            u = (int)(u * map.width);
            v = (int)(v * map.height);
            
            float opacity = ((float)(map.pixels[(int)u + (int)v * 255] & 255)) / 256;

            return new Vector3(1f, 1f, 1f) * opacity;
        }

        internal Intersection PrimaryRayIntersection(Ray ray, Scene scene)
        {
            Intersection intersection = null;
            Intersection tempIntersection;
            bool collided = false;
            int primitivesCount = scene.primitives.Count;
            for (int i = 0; i < primitivesCount; i++)
            {
                tempIntersection = null;
                if (scene.primitives[i] is Plane)
                {
                    tempIntersection = collideRayPlane(ray, scene.primitives[i] as Plane);
                }
                if (scene.primitives[i] is Sphere)
                {
                    tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere);
                    if (tempIntersection.distance < Application.epsilon)
                        tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere, true);
                }
                else if (scene.primitives[i] is Triangle)
                {
                    tempIntersection = collideRayTriangle(ray, scene.primitives[i] as Triangle);
                }
                if (tempIntersection != null)
                    if (tempIntersection.nearestPrimitive != null && tempIntersection.distance > Application.epsilon && (intersection == null || tempIntersection.distance < intersection.distance - Application.epsilon))
                    {
                        collided = true;
                        intersection = tempIntersection;
                    }
            }
            if (collided)
                return intersection;
            else
                return null;
        }

        internal Intersection ShadowRayIntersection(Ray ray, Scene scene)
        {
            float distance = 0;
            Primitive nearestPrimitive = null;
            Vector3 normal = Vector3.Zero;
            Vector3 pointOfIntersection = Vector3.Zero;
            int primitivesCount = scene.primitives.Count;
            Intersection tempIntersection = null;
            bool intersected = false;
            for (int i = 0; i < primitivesCount && !intersected; i++)
            {
                if (scene.primitives[i] is Plane)
                {
                    tempIntersection = collideRayPlane(ray, scene.primitives[i] as Plane);
                }
                else if (scene.primitives[i] is Sphere)
                {
                    tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere);
                    if (!(tempIntersection.distance > Application.epsilon && tempIntersection.distance < ray.intersectionDistance - Application.epsilon))
                        tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere, true);
                }
                else if (scene.primitives[i] is Triangle)
                {
                    tempIntersection = collideRayTriangle(ray, scene.primitives[i] as Triangle);
                }
                if (tempIntersection.distance > Application.epsilon && tempIntersection.distance < ray.intersectionDistance - Application.epsilon)
                {
                    intersected = true;
                    distance = tempIntersection.distance;
                    nearestPrimitive = scene.primitives[i];
                    normal = tempIntersection.normal;
                    pointOfIntersection = tempIntersection.position;
                }
            }
            if (intersected)
                return tempIntersection;
            else
                return null;
        }

        internal Vector3 FindShadowRayColor(Ray ray, Light light, Scene scene)
        {
            int primitivesCount = scene.primitives.Count;
            Intersection tempIntersection = null;
            for (int i = 0; i < primitivesCount; i++)
            {
                if (scene.primitives[i] is Plane)
                {
                    tempIntersection = collideRayPlane(ray, scene.primitives[i] as Plane);
                }
                else if (scene.primitives[i] is Sphere)
                {
                    tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere);
                    if (!(tempIntersection.distance > Application.epsilon && tempIntersection.distance < ray.intersectionDistance - Application.epsilon))
                        tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere, true);
                }
                else if (scene.primitives[i] is Triangle)
                {
                    tempIntersection = collideRayTriangle(ray, scene.primitives[i] as Triangle);
                }
                if (tempIntersection.distance > Application.epsilon && tempIntersection.distance < ray.intersectionDistance - Application.epsilon)
                {
                    return Vector3.Zero;
                }
            }
            return light.rgbIntensity;
        }

        internal Intersection collideRayTriangle(Ray ray, Triangle primitive)
        {
            Intersection tempIntersection = collideRayPlane(ray, new Plane(primitive.normal, primitive.pointA, primitive.materialColor, primitive.speculalColor, primitive.specularity));

            if (tempIntersection.distance != 0) //Hits the plane of the triangle
            {
                float alpha = Vector3.Dot(Vector3.Cross((primitive.pointC - primitive.pointB), (tempIntersection.position - primitive.pointB)), primitive.normal) / Vector3.Dot(Vector3.Cross((primitive.pointB - primitive.pointA), (primitive.pointC - primitive.pointA)), primitive.normal);
                float beta = Vector3.Dot(Vector3.Cross((primitive.pointA - primitive.pointC), (tempIntersection.position - primitive.pointC)), primitive.normal) / Vector3.Dot(Vector3.Cross((primitive.pointB - primitive.pointA), (primitive.pointC - primitive.pointA)), primitive.normal);
                float gamma = Vector3.Dot(Vector3.Cross((primitive.pointB - primitive.pointA), (tempIntersection.position - primitive.pointA)), primitive.normal) / Vector3.Dot(Vector3.Cross((primitive.pointB - primitive.pointA), (primitive.pointC - primitive.pointA)), primitive.normal);

                if (0 <= alpha && alpha <= 1 && 0 <= beta && beta <= 1 && 0 <= gamma && gamma <= 1) //Point is inside triangle
                {
                    float length = (tempIntersection.position - ray.origin).Length;
                    return new TriangleIntersection(length, primitive, primitive.normal, tempIntersection.position, alpha, beta, gamma);
                }
                else //Point is not in triangle
                {
                    return new Intersection(0, null, Vector3.Zero, Vector3.Zero);
                }
            }
            else //Does not hit the plane of the triangle
            {
                return new Intersection(0, null, Vector3.Zero, Vector3.Zero);
            }
        }

        internal Intersection collideRayPlane(Ray ray, Plane primitive)
        {
            float number = Vector3.Dot(ray.direction, primitive.normal);
            if (number < 1)
            {
                float distance = Vector3.Dot(primitive.distanceToOrigin - ray.origin, primitive.normal) / number;
                Vector3 position = ray.origin + ray.direction * distance;
                return new Intersection(distance, primitive, primitive.normal, position);
            }
            return new Intersection(0, null, Vector3.Zero, Vector3.Zero);
        }

        internal Intersection collideRaySphere(Ray ray, Sphere primitive, bool secondaryCollision = false)
        {
            float length = 0;
            Vector3 pointOfIntersection = Vector3.Zero;
            Vector3 tempNormal = Vector3.Zero;
            ray.direction.Normalize();
            float a = ray.direction.X * ray.direction.X + ray.direction.Y * ray.direction.Y + ray.direction.Z * ray.direction.Z;
            float b = (2 * (ray.origin.X - primitive.position.X) * ray.direction.X) + (2 * (ray.origin.Y - primitive.position.Y) * ray.direction.Y) + (2 * (ray.origin.Z - primitive.position.Z) * ray.direction.Z);
            float c = (ray.origin.X - primitive.position.X) * (ray.origin.X - primitive.position.X) + (ray.origin.Y - primitive.position.Y) * (ray.origin.Y - primitive.position.Y) + (ray.origin.Z - primitive.position.Z) * (ray.origin.Z - primitive.position.Z) - primitive.radius * primitive.radius;

            float t = 0;
            float d = b * b - 4 * a * c;
            if (d >= 0 && a >= 0) //If there are solutions
            {
                if (!secondaryCollision)
                    t = (float)((-b - Math.Sqrt(d)) / (2 * a));
                else
                    t = (float)((-b + Math.Sqrt(d)) / (2 * a));

                if (t > 0)
                {
                    pointOfIntersection = ray.origin + ray.direction * t;
                    length = (ray.direction * t).Length;
                    tempNormal = pointOfIntersection - primitive.position;
                    tempNormal.Normalize();
                }
            }
            return new Intersection(length, primitive, tempNormal, pointOfIntersection);
        }
        internal Ray FindPrimaryRay(float x, float y, float width, float height)
        {
            Vector3 u = camera.rightTop - camera.leftTop;
            Vector3 v = camera.leftBottom - camera.leftTop;

            Vector3 screenPoint = camera.leftTop + (x / width) * u + (y / height) * v;
            Vector3 rayDirection = screenPoint - camera.position;
            float length = rayDirection.Length;
            rayDirection.Normalize();
            Ray ray = new Ray(camera.position, rayDirection, length);
            return ray;
        }
        internal Ray FindShadowRay(Intersection intersection, Light light)
        {
            Vector3 rayDirection = light.position - intersection.position;
            float length = rayDirection.Length;
            rayDirection.Normalize();
            return new Ray(intersection.position, rayDirection, length);
        }

        internal Ray FindRefractionRay(Intersection intersection, Ray ray)
        {
            Vector3 d = ray.direction;
            Vector3 n = intersection.normal;
            float pOD = ray.opticalDensity; //Previous Optical Density
            float nOD = intersection.nearestPrimitive.opticalDensity; //Next Optical Density
            if (Vector3.Dot(n, d) < 0) //Moving out of a sphere
            {
                n *= -1;
                nOD = scene.sceneOpticalDensity;
            }
            float cosT = Vector3.Dot(d, n);
            float sqrt = 1 - ((pOD * pOD) / (nOD * nOD)) * (1 - cosT * cosT);
            if(sqrt >= 0)
            {
                Vector3 t = (pOD / nOD) * (d + cosT * n) - (float)Math.Sqrt(sqrt) * n;
                t.Normalize();
                return new Ray(intersection.position, t, 0, ray.numberOfBounces + 1, nOD);
            }
            else //Doesn't refract
            {
                return null;
            }
        }
        private int TX(float X)
        {
            return (int)((-X+camera.position.X) * 60 + screen.width / 2);
        }

        private int TY(float Z)
        {
            return (int)((Z+camera.position.Z) * 60 + screen.height * 0.9f);
        }

        internal Vector3 Background(Vector3 direction)
        {
            double theta = 90;
            double phi = 90;
            if (direction.Z != 0)
                theta = MathHelper.RadiansToDegrees(Math.Atan(direction.X / direction.Z));
            if(direction.Z != 0)
                phi = MathHelper.RadiansToDegrees(Math.Atan(direction.Y / direction.Z));

            if(direction.Z > 0)
            {
                phi *= -1;
            }

            float scaleX = background.width / 180f;
            float scaleY = background.height / 180f;

            int u = Math.Clamp((int)((theta + 90) * scaleX), 0, background.width - 1);
            int v = Math.Clamp((int)((phi + 90) * scaleY), 0, background.height - 1);

            int intColor = background.pixels[(int)u + (int)v * background.width];

            Color color = Color.FromArgb(intColor);   //https://stackoverflow.com/a/6131464

            float red = color.R / 255f;
            float green = color.G / 255f;
            float blue = color.B / 255f;
            return new Vector3(red, green, blue);
        }
    }
}
