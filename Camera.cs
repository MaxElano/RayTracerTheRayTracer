using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenTK.Graphics.OpenGL.GL;

namespace RayTracer
{
    //tracer:Camera, with data members position,look-atdirection, and up direction.
    //The camera also stores the screen plane, specified by its four corners, which are updated whenever camera position and/or direction is modified.
    //Hardcoded coordinates and directions allow for an easy start.Use e.g. (0,0,0) as the camera origin, (0,0,1) as the look-at direction, and (0,1,0) as the up direction;
    //this way the screen corners can also be hardcoded for the time being.
    //Once the basic setup works, you mustmake this more flexible.
    internal class Camera
    {
        internal Vector3 position;
        internal Vector3 lookAtDirection;
        Vector3 upDirection;
        internal Vector3 rightDirection;
        internal float distanceToScreenPlane; 
        internal Vector3 screenPlaneCenter;

        public Vector3 leftTop;
        public Vector3 rightTop;
        public Vector3 leftBottom;
        public Vector3 rightBottom;

        float resolution;

        Surface screen;
        internal Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection, float distanceToScreenPlane, Surface screen)
        {
            this.position = position;
            this.lookAtDirection = lookAtDirection;
            this.upDirection = upDirection;
            this.distanceToScreenPlane = distanceToScreenPlane;

            rightDirection = Vector3.Cross(lookAtDirection, upDirection);

            screenPlaneCenter = position + distanceToScreenPlane * lookAtDirection;

            resolution = (float)screen.width / (float)screen.height;
            SetScreenPlaneCorners();
        }
        internal void SetScreenPlaneCorners()
        {
            screenPlaneCenter = position + distanceToScreenPlane * lookAtDirection;
            leftTop = screenPlaneCenter + upDirection - resolution * rightDirection;
            rightTop = screenPlaneCenter + upDirection + resolution * rightDirection;
            leftBottom = screenPlaneCenter - upDirection - resolution * rightDirection;
            rightBottom = screenPlaneCenter - upDirection + resolution * rightDirection;
        }

        internal void FixRightDirection()
        {
            rightDirection = Vector3.Cross(lookAtDirection, upDirection);
            rightDirection.Normalize();
        }

        internal void FixUpDirection()
        {
            upDirection = Vector3.Cross(lookAtDirection, rightDirection) * -1;
            upDirection.Normalize();
        }
    }
}
