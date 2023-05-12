using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //Light, which stores the location and intensity of a light source.For a Whitted-style ray tracer, this will be a point light.
    //Intensity should be stored using float values for red, green and blue.
    internal class Light
    {
        internal Vector3 position;
        internal Vector3 rgbIntensity;
        internal Light(Vector3 position, Vector3 rgbIntensity)
        {
            this.position = position;
            this.rgbIntensity = rgbIntensity;
        }
    }
}
