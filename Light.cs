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

    internal class Spotlight : Light
    {
        Vector3 shineAtDirection;
        double angle;
        internal Spotlight(Vector3 position, Vector3 rgbIntensity, Vector3 shineAtDirection, float angle) : base(position, rgbIntensity)
        {
            this.shineAtDirection = shineAtDirection;
            shineAtDirection.Normalize();
            this.angle = angle;
        }

        internal bool DoesItHit(Ray ray)
        {
            double incomingAngle = MathHelper.RadiansToDegrees(Math.Acos(Vector3.Dot(-shineAtDirection, ray.direction)));
            if (incomingAngle <= angle)
                return true;
            else
                return false;
        }
    }
}
