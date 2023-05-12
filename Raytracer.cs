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
        internal Camera camera;
        Surface screen;
        internal Raytracer(Surface screen)
        {
            scene = new Scene();
            camera = new Camera(new Vector3(0,0,0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 1f, screen);
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
                    if(x == width/2 && y == height/2)
                    {

                    }
                    screen.pixels[x + y * width] = 0;
                    Ray ray = findRay(x, y, width, height);
                    Intersection closestIntersection = scene.IntersectClosestObject(ray);
                    if(closestIntersection.nearestPrimitive != null)
                        screen.pixels[x + y * width] = (int)Math.Round(closestIntersection.nearestPrimitive.rgbIntensity.X * 255 * 256 * 256 + closestIntersection.nearestPrimitive.rgbIntensity.Y * 255 * 256 + closestIntersection.nearestPrimitive.rgbIntensity.Z * 255);
                }
            }

        }

        internal Ray findRay(float x, float y, float width, float height)
        {
            Vector3 u = camera.rightTop - camera.leftTop;
            Vector3 v = camera.leftBottom - camera.leftTop;

            Vector3 screenPoint = camera.leftTop + (x / width) * u + (y / height) * v;
            Vector3 rayDirection = screenPoint - camera.position;
            rayDirection.Normalize();
            Ray ray = new Ray(camera.position, rayDirection);
            return ray;
        }
    }
}
