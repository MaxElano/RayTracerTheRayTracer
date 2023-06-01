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
        internal float sceneOpticalDensity = Primitive.DetermineOpticalDensity(Primitive.Materials.air);
        internal Scene()
        {
            primitives = new List<Primitive>();
            lights = new List<Light>();

            //primitives.Add(new TexturedPlane(new Vector3(0,1,0), new Vector3(0, -2, 0), new Vector3(0.6f,0.6f,0.6f), new Vector3(0.8f,0.8f,0.8f)));
            //primitives.Add(new Plane(new Vector3(0, 1, 0), new Vector3(0, -2, 0), new Vector3(0.6f, 0.6f, 0.6f), new Vector3(0.8f, 0.8f, 0.8f)));

            //primitives.Add(new Plane(new Vector3(0,0,1), new Vector3(0, 0, 2), new Vector3(0.2f,0.2f,0.2f), new Vector3(0,0,0)));

            //primitives.Add(new Sphere(new Vector3(0, 0, 4), 1f, new Vector3(1, 0, 0), new Vector3(0.8f, 0.8f, 0.8f)));
            primitives.Add(new Sphere(new Vector3(4, 0, 6.5f), 1f, new Vector3(0, 0, 1), new Vector3(0.8f, 0.8f, 0.8f)));

            primitives.Add(new Sphere(new Vector3(0, 0, 2), 1f, new Vector3(1, 0, 0), new Vector3(0.8f, 0.8f, 0.8f)));
            //primitives.Add(new TexturedTriangle(new Vector3(-2, 0, 2), new Vector3(2, 0, 2), new Vector3(0, 5, 2), new Vector3(1, 1, 1), new Vector3(1, 1, 1)));

            //Sphere window = new Sphere(new Vector3(2.5f, 0, 3), 1f, new Vector3(1, 1, 1), new Vector3(0.8f, 0.8f, 0.8f), 0, Primitive.Materials.window_glass);
            //window.specularColor = new Vector3(0.2f, 0.2f, 0.2f);
            //primitives.Add(window);

            Sphere mirror = new Sphere(new Vector3(2.5f, 2.5f, 3), 1f, new Vector3(1, 1, 1), new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
            mirror.specularColor = new Vector3(0.7f, 0.7f, 0.7f);
            primitives.Add(mirror);
            primitives.Add(new TexturedSphere(new Vector3(0, 2.5f, 4), 1f, new Vector3(1, 0, 0), new Vector3(0.8f, 0.8f, 0.8f)));

            lights.Add(new Light(new Vector3(4, 0, -1), new Vector3(10, 10, 10)));
            lights.Add(new Spotlight(new Vector3(0, 10, 4.5f), new Vector3(50, 50, 50), new Vector3(0, -1, 0), 15f));
        }
    }
}
