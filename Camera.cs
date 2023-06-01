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
        internal Vector3 upDirection;
        internal Vector3 rightDirection;
        internal float distanceToScreenPlane; 
        internal Vector3 screenPlaneCenter;
        

        public Vector3 leftTop;
        public Vector3 rightTop;
        public Vector3 leftBottom;
        public Vector3 rightBottom;

        internal float resolution;
        internal float fov, pitch, yaw;

        internal Surface screen;
        internal Camera(Vector3 position, Vector3 lookAtDirection, Vector3 upDirection, float distanceToScreenPlane, Surface screen)
        {
            this.position = position;
            this.lookAtDirection = lookAtDirection;
            this.upDirection = upDirection;
            this.distanceToScreenPlane = distanceToScreenPlane;
            this.screen = screen;

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

        internal void SetFrontDirection()
        {
            lookAtDirection = new Vector3(
                (float)(Math.Sin(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch))),
                (float)Math.Sin(MathHelper.DegreesToRadians(pitch)),
                (float)(-Math.Cos(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch)))
            );
            lookAtDirection.Normalize();
        }

        internal void SetRightDirection()
        {
            rightDirection = new Vector3(
                (float)Math.Cos(MathHelper.DegreesToRadians(yaw)),
                0f,
                (float)Math.Sin(MathHelper.DegreesToRadians(yaw))
            );
            rightDirection.Normalize();
        }

        internal void SetUpDirection()
        {
            upDirection = Vector3.Cross(rightDirection, lookAtDirection);
            upDirection.Normalize();
        }

        internal void LookAt(Primitive target)
        {
            if (target is Sphere)
            {
                var sphere = (Sphere)target;
                lookAtDirection = new Vector3(position.X - sphere.position.X, sphere.position.Y - position.Y, sphere.position.Z - position.Z);
            }
            else if (target is Plane)
            {
                var plane = (Plane)target;
                Vector3 nearestPoint = FindNearestPoint(plane);
                lookAtDirection = new Vector3(position.X - nearestPoint.X, nearestPoint.Y - position.Y, nearestPoint.Z - position.Z);
            }
            else if (target is Triangle)
            {
                var triangle = (Triangle)target;
                Vector3 triangleCenter = (triangle.pointA + triangle.pointB + triangle.pointC) / 3;
                lookAtDirection = new Vector3(position.X - triangleCenter.X, triangleCenter.Y - position.Y, triangleCenter.Z - position.Z);
            }
            lookAtDirection.Normalize();
            CalculateNewPitchYaw();

            SetFrontDirection();
            SetRightDirection();
            SetUpDirection();
        }

        internal void CalculateNewPitchYaw()
        {
            pitch = (float)MathHelper.RadiansToDegrees(Math.Asin(lookAtDirection.Y));
            double tempX = lookAtDirection.X / Math.Cos(MathHelper.DegreesToRadians(pitch));
            double tempZ = lookAtDirection.Z / Math.Cos(MathHelper.DegreesToRadians(pitch));
            yaw = (float)MathHelper.RadiansToDegrees(Math.Atan2(tempX, tempZ)) + 180f;
        }

        internal Vector3 FindNearestPoint(Plane plane)
        {
            Vector3 normalizedVector = plane.normal.Normalized();
            float divider = normalizedVector.X * plane.normal.X + normalizedVector.Y * plane.normal.Y + normalizedVector.Z * plane.normal.Z;
            float n = plane.normal.X * position.X + plane.normal.Y * position.Y + plane.normal.Z * position.Z;
            float D = -(plane.normal.X * plane.distanceToOrigin.X + plane.normal.Y * plane.distanceToOrigin.Y + plane.normal.Z *  plane.distanceToOrigin.Z);
            float multiplier = -(n + D) / divider;
            Vector3 result = new Vector3(
                multiplier * normalizedVector.X + position.X,
                multiplier * normalizedVector.Y + position.Y,
                multiplier * normalizedVector.Z + position.Z
            );
            return result;
        }

        internal void SetFOV(float halfPlane, float angle)
        {
            distanceToScreenPlane = halfPlane / (float)Math.Tan(MathHelper.DegreesToRadians(angle));
            fov = CalculateFOV(resolution, distanceToScreenPlane);

        }

        internal float CalculateFOV(float halfPlane, float disPlane)
        {
            return (float)MathHelper.RadiansToDegrees(Math.Atan(halfPlane / disPlane));
        }
    }
}
