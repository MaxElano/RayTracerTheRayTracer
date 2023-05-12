using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //Application, which calls the Render method of the Raytracer.
    //The application is responsible for handling keyboard and/or mouse input.
    internal class Application
    {
        Raytracer raytracer;
        static internal float epsilon = 0.0001f;
        internal Application(Surface screen)
        {
            raytracer = new Raytracer(screen);
        }
        internal void Update()
        {
            raytracer.Render();
        }
    }
}
