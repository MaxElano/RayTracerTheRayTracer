using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Surface screen;

        bool debugMode;
        internal Raytracer(Surface screen)
        {
            scene = new Scene();
            camera = new Camera(new Vector3(0,0,0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 1f, screen);
            debugMode = false;
            this.screen = screen;
        }
        internal void Render()
        {
            int width = screen.width;
            int height = screen.height;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    screen.pixels[x + y * width] = 0;
                    Ray primaryRay = findPrimaryRay(x, y, width, height);
                    Intersection primaryIntersection = scene.PrimaryRayIntersection(primaryRay);

                    if (debugMode) //Test debugMode (WIP)
                    {
                        
                    }
                    else
                    {
                        if (primaryIntersection.nearestPrimitive != null)
                        {
                            Vector3 color = Vector3.Zero;
                            int lightsCount = scene.lights.Count();
                            for (int i = 0; i < lightsCount; i++)
                            {
                                Light light = scene.lights[i];
                                Ray shadowRay = FindShadowRay(primaryIntersection, light);
                                Vector3 intensity = scene.FindShadowRayColor(shadowRay, light);
                                float distance = shadowRay.intersectionDistance;
                                Vector3 normal = primaryIntersection.normal;
                                Vector3 materialColor = primaryIntersection.nearestPrimitive.materialColor;
                                Vector3 specularColor = primaryIntersection.nearestPrimitive.speculalColor;
                                Vector3 r = -shadowRay.direction - 2 * Vector3.Dot(-shadowRay.direction, primaryIntersection.normal) * primaryIntersection.normal;
                                r.Normalize();
                                float n = 2;
                                Vector3 materialsAmbientColor = materialColor;
                                Vector3 ambientLightRadiance = new Vector3(0.4f, 0.4f, 0.4f);
                                color = light.rgbIntensity * (1 / (distance * distance)) * Math.Max(0, Vector3.Dot(normal, shadowRay.direction)) * materialColor;
                                //color = light.rgbIntensity * (1 / (distance * distance)) * (materialColor * Math.Max(0, Vector3.Dot(normal, shadowRay.direction)) + specularColor * (float)Math.Pow(Math.Max(0, Vector3.Dot(-primaryRay.direction, r)), n)) + materialsAmbientColor * ambientLightRadiance;
                            }
                            screen.pixels[x + y * width] = (int)Math.Round(color.X * 255 * 256 * 256 + color.Y * 255 * 256 + color.Z * 255);
                        }
                    }
                }
            }

        }

        internal Ray findPrimaryRay(float x, float y, float width, float height)
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
    }
}
