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
        }
    }
}
