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
        Camera camera;
        Surface screen;
        internal Raytracer(Surface screen)
        {
            scene = new Scene();
            camera = new Camera(new OpenTK.Mathematics.Vector3(0,0,0), new OpenTK.Mathematics.Vector3(0, 0, 1), new OpenTK.Mathematics.Vector3(0, 1, 0));
            this.screen = screen;
        }
        internal void Render()
        {
            
        }
    }
}
