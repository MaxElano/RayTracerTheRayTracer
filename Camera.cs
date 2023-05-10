using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracer
{
    //tracer:Camera, with data members position,look-atdirection, and up direction.
    //The camera also stores the screen plane, specified by its four corners, which are updated whenever camera position and/or direction is modified.
    //Hardcoded coordinates and directions allow for an easy start.Use e.g. (0,0,0) as the camera origin, (0,0,1) as the look-at direction, and (0,1,0) as the up direction;
    //this way the screen corners can also be hardcodedfor the time being.
    //Once the basic setup works, you mustmake this more flexible.
    internal class Camera
    {
        Vector3 position;
        Vector3 lookAtDirection;
        Vector3 upDirection;

        public Vector3 leftTop;
        public Vector3 rightTop;
        public Vector3 leftBottom;
        public Vector3 rightBottom;

        
        internal Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection)
        {
            this.position = position;
            this.lookAtDirection = lookAtDirection;
            this.upDirection = upDirection;
        }
    }
}
