using Microsoft.VisualBasic;
using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Data;
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
        Scene scene;
        Camera debugCamera;
        internal Camera camera;
        Surface screen, map;

        List<float> tempDistances = new List<float>();
        float[,] tempArray = new float[2, 10000];
        int tempint = 0;
        Ray primaryRay;
        Intersection primaryIntersection;

        int numberOfBouncesAllowed = 5;

        internal static object lockObject = new object();

        internal bool debugMode, multiThreading = true;
        internal Raytracer(Surface screen)
        {
            scene = new Scene();
            camera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 1f, screen);
            debugMode = false;
            this.screen = screen;
            map = new Surface("../../../assets/sus.png");
        }
        internal void Render()
        {
            if (debugMode)
            {
                screen.Clear(0x000000);

                screen.Line(TX(camera.leftBottom.X), TY(-camera.position.Z - camera.distanceToScreenPlane), TX(camera.rightBottom.X), TY(-camera.position.Z - camera.distanceToScreenPlane), 0xffffff);
                screen.Box(TX(camera.position.X)-1, TY(-camera.position.Z)+1, TX(camera.position.X)+1, TY(-camera.position.Z)-1, 0xffffff);

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

                for (int x = 0; x < screen.width; x++)
                    if (x % 10 == 0 || x == 0)
                    {
                        primaryRay = FindPrimaryRay(x, screen.height / 2, screen.width, screen.height);
                        primaryIntersection = PrimaryRayIntersection(primaryRay, scene);
                        if (primaryIntersection != null /*&& primaryIntersection.nearestPrimitive is Sphere*/)
                            screen.Line(TX(camera.position.X), TY(-camera.position.Z), TX(primaryIntersection.position.X), TY(-primaryIntersection.position.Z), 0xfcba03);
                        else
                            screen.Line(TX(camera.position.X), TY(-camera.position.Z), TX(primaryRay.direction.X * 100), TY(-primaryRay.direction.Z * 100), 0xfcba03);

                        if (primaryIntersection != null)
                        {
                            int lightsCount = scene.lights.Count();
                            Ray shadowRay;
                            Light light;
                            Intersection shadowIntersection;
                            for (int i = 0; i < lightsCount; i++)
                            {
                                light = scene.lights[i];
                                shadowRay = FindShadowRay(primaryIntersection, light);
                                shadowIntersection = PrimaryRayIntersection(shadowRay, scene);

                                if (shadowIntersection == null)
                                    screen.Line(TX(primaryIntersection.position.X), TY(-primaryIntersection.position.Z), TX(light.position.X), TY(-light.position.Z), 0xff1100);
                                else
                                    screen.Line(TX(primaryIntersection.position.X), TY(-primaryIntersection.position.Z), TX(shadowIntersection.position.X), TY(-shadowIntersection.position.Z), 0xff1100);
                            }

                            Ray refractedRay = FindRefractionRay(primaryIntersection, primaryRay);
                            Intersection refractedIntersection = primaryIntersection;
                            float refractedR = 1;
                            while (refractedRay is not null && refractedRay.numberOfBounces < numberOfBouncesAllowed && refractedIntersection.nearestPrimitive != null)
                            {
                                refractedIntersection = PrimaryRayIntersection(refractedRay, scene, true);
                                if (refractedIntersection != null)
                                    refractedIntersection.distance = 90;
                                Vector3 normal = primaryIntersection.normal;
                                if (Vector3.Dot(normal, refractedRay.direction) > 0)
                                    normal *= -1;
                                //float tempR = ((primaryIntersection.nearestPrimitive.opticalDensity - primaryRay.opticalDensity) / (primaryIntersection.nearestPrimitive.opticalDensity + primaryRay.opticalDensity)) * ((primaryIntersection.nearestPrimitive.opticalDensity - primaryRay.opticalDensity) / (primaryIntersection.nearestPrimitive.opticalDensity + primaryRay.opticalDensity));
                                //refractedR = tempR + (1 - tempR) * (float)(Math.Pow(1 - Vector3.Dot(primaryRay.direction, normal), 5));
                                screen.Line(TX(refractedRay.origin.X), TY(-refractedRay.origin.Z), TX(refractedRay.origin.X + refractedRay.direction.X * refractedIntersection.distance), TY(-(refractedRay.origin.Z + refractedRay.direction.Z * refractedIntersection.distance)), 0x1AAAAA);
                                if(refractedIntersection.nearestPrimitive != null)
                                    refractedRay = FindRefractionRay(refractedIntersection, refractedRay);
                            }
                        }
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
                            Vector3 color;
                            color = Trace(primaryRay, scene);
                            int tempColor = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256 + ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256 + (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                            screen.pixels[x + y * screen.width] = tempColor;
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

                        if (primaryIntersection != null && primaryIntersection.distance > 0 && primaryIntersection.distance < 100)
                        {
                            Vector3 color = Trace(primaryRay, scene);
                            int tempColor = ((int)Math.Round(Math.Clamp(color.X, 0, 1) * 255)) * 256 * 256 + ((int)Math.Round(Math.Clamp(color.Y, 0, 1) * 255)) * 256 + (int)Math.Round(Math.Clamp(color.Z, 0, 1) * 255);
                            screen.pixels[x + y * screen.width] = tempColor;
                        }
                        else
                        {
                            screen.pixels[x + y * screen.width] = 0;
                        }
                    }
                }
            }
        }
        internal Vector3 Trace(Ray ray, Scene scene) //Might Move the things from scene into raytracer for easier code but isn't easier yet so wip
        {
            Vector3 color = Vector3.Zero;

            //-------------Start Searching For Closest Primitive-------------
            int primitivesCount = scene.primitives.Count;
            Intersection intersection = new Intersection(-1, null, Vector3.Zero, Vector3.Zero);
            Intersection tempIntersection, tempIntersection2;

            for (int i = 0; i < primitivesCount; i++)
            {
                tempIntersection = null;
                if (scene.primitives[i] is Plane)
                {
                    tempIntersection = collideRayPlane(ray, scene.primitives[i] as Plane);
                }
                else if (scene.primitives[i] is Sphere)
                {
                    tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere);
                    if (tempIntersection.distance < Application.epsilon)
                        tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere, true);
                }
                else if (scene.primitives[i] is Triangle)
                {
                    tempIntersection = collideRayTriangle(ray, scene.primitives[i] as Triangle);
                }
                if (tempIntersection.distance > Application.epsilon && (tempIntersection.distance < intersection.distance - Application.epsilon || intersection.distance == -1)) //If it hits something that is not itself
                {
                    intersection = tempIntersection;
                }
            }

            //-------------Ray Hit Something So Do...-------------
            if (intersection.nearestPrimitive != null)
            {
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


                    double uP = triangle.alpha * uA + triangle.beta * uB + triangle.gamma * uC;
                    double vP = triangle.alpha * vA + triangle.beta * vB + triangle.gamma * vC;

                    materialColor = CheckboardPattern(uP, vP, 32);
                }
                else if (intersection.nearestPrimitive is TexturedPlane)
                {
                    var plane = (TexturedPlane)intersection.nearestPrimitive;
                    Vector3 vectorU = new Vector3(-1, 0, 0);
                    Vector3 vectorV = Vector3.Normalize(Vector3.Cross(Vector3.Normalize(plane.normal), Vector3.Normalize(vectorU)));
                    Double u = intersection.position.X / vectorU.X;
                    Double v = intersection.position.Z / vectorV.Z;
                    int uInt = (int)u;
                    int vInt = (int)v;
                    u = u - uInt;
                    v = v - vInt;

                    materialColor = CheckboardPattern(u, v, 4);
                }

                if (intersection.nearestPrimitive.specularity == 1 && ray.numberOfBounces < 10) //Pure specular
                {
                    Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                    reflectedVector.Normalize();
                    Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);
                    color += materialColor * Trace(reflectedRay, scene);
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

                    for (int i = 0; i < lightsCount; i++)
                    {
                        light = scene.lights[i];
                        shadowRay = FindShadowRay(intersection, light);
                        intensity = FindShadowRayColor(shadowRay, light, scene);
                        distance = shadowRay.intersectionDistance;

                        r = -shadowRay.direction - 2 * Vector3.Dot(-shadowRay.direction, intersection.normal) * intersection.normal;
                        r.Normalize();

                        n = 20;

                        if (intersection.nearestPrimitive.specularity != 0 && ray.numberOfBounces < numberOfBouncesAllowed) //Partly specular
                        {
                            Vector3 reflectedVector = ray.direction - 2 * Vector3.Dot(ray.direction, intersection.normal) * intersection.normal;
                            reflectedVector.Normalize();
                            Ray reflectedRay = new Ray(intersection.position, reflectedVector, 0, ray.numberOfBounces + 1);
                            Vector3 tempColor = intersection.nearestPrimitive.specularColor * Trace(reflectedRay, scene);
                            color += light.rgbIntensity * (1 / (distance * distance)) * materialColor * Math.Max(0, Vector3.Dot(intersection.normal, shadowRay.direction)) + tempColor;
                        }
                        else if (intensity != Vector3.Zero)
                        {
                            color += light.rgbIntensity * (1 / (distance * distance)) * (materialColor * Math.Max(0, Vector3.Dot(intersection.normal, shadowRay.direction)) + intersection.nearestPrimitive.speculalColor * (float)Math.Pow(Math.Max(0, Vector3.Dot(-ray.direction, r)), n));
                        }
                    }

                    Vector3 materialsAmbientColor = materialColor;
                    Vector3 ambientLightRadiance = new Vector3(0.05f, 0.05f, 0.05f);

                    color += materialsAmbientColor * ambientLightRadiance;
                }

                Ray refractedRay = FindRefractionRay(intersection, ray);
                float refractedR = 1;
                if (refractedRay is not null && ray.numberOfBounces < numberOfBouncesAllowed)
                {
                    Vector3 normal = intersection.normal;
                    if (Vector3.Dot(normal, ray.direction) > 0)
                    {
                        normal *= -1;
                    }
                    float pOD = ray.opticalDensity; //Previous Optical Density
                    float nOD = refractedRay.opticalDensity; //Next Optical Density
                    float tempR = ((nOD - pOD) / (nOD + pOD)) * ((nOD - pOD) / (nOD + pOD));
                    refractedR = tempR + (1 - tempR) * (float)(Math.Pow(1 - Vector3.Dot(ray.direction, normal), 5));
                    color *= refractedR;
                    color += (1 - refractedR) * Trace(refractedRay, scene);
                }

                //if (intersection.nearestPrimitive is Triangle)
                //{
                //    color = new Vector3(color.X * (intersection.nearestPrimitive as Triangle).alpha, color.Y * (intersection.nearestPrimitive as Triangle).beta, color.Z * (intersection.nearestPrimitive as Triangle).gamma);
                //}


                return color;
            }
            else
            {
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
            float distance = 0;
            Primitive nearestPrimitive = null;
            Vector3 normal = Vector3.Zero;
            Vector3 pointOfIntersection = Vector3.Zero;
            int primitivesCount = scene.primitives.Count;
            for (int i = 0; i < primitivesCount; i++)
            {
                Intersection tempIntersection = null;
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
                    if (tempIntersection.nearestPrimitive != null && tempIntersection.distance > Application.epsilon && (tempIntersection.distance < distance - Application.epsilon || distance == 0))
                    {
                        distance = tempIntersection.distance;
                        nearestPrimitive = scene.primitives[i];
                        normal = tempIntersection.normal;
                        pointOfIntersection = tempIntersection.position;
                    }
            }
            Intersection intersection = new Intersection(distance, nearestPrimitive, normal, pointOfIntersection);

            return intersection;
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
                    if (tempIntersection.distance == 0)
                        tempIntersection = collideRaySphere(ray, scene.primitives[i] as Sphere, true);
                }
                else if (scene.primitives[i] is Triangle)
                {
                    tempIntersection = collideRayTriangle(ray, scene.primitives[i] as Triangle);
                }
                if (tempIntersection.distance > Application.epsilon && tempIntersection.distance < ray.intersectionDistance - Application.epsilon)
                {
                    distance = tempIntersection.distance;
                    nearestPrimitive = scene.primitives[i];
                    normal = tempIntersection.normal;
                    pointOfIntersection = tempIntersection.position;
                }
            }
            Intersection intersection = new Intersection(distance, nearestPrimitive, normal, pointOfIntersection);

            return intersection;
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
                    primitive.alpha = alpha;
                    primitive.beta = beta;
                    primitive.gamma = gamma;
                    float length = (tempIntersection.position - ray.origin).Length;
                    return new Intersection(length, primitive, primitive.normal, tempIntersection.position);
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
            if (Vector3.Dot(n, d) > 0) //Moving out of a sphere
            {
                n *= -1;
                nOD = scene.sceneOpticalDensity;
            }
            float cosT = Math.Clamp(Vector3.Dot(d, n), 0, 1);
            float sqrt = 1 - ((pOD * pOD) / (nOD * nOD)) * (1 - cosT * cosT);
            if(sqrt >= 0)
            {
                Vector3 t = (pOD / nOD) * (d + cosT * n) - (float)Math.Sqrt(sqrt) * n;
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
    }
}
