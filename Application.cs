
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTK.Mathematics;

namespace RayTracer
{
    //Application, which calls the Render method of the Raytracer.
    //The application is responsible for handling keyboard and/or mouse input.
    internal class Application
    {
        Raytracer raytracer;
        KeyboardState keyboard;
        static internal float epsilon = 0.0001f;
        private float yaw, pitch;

        internal Application(Surface screen, KeyboardState keyboard)
        {
            raytracer = new Raytracer(screen);
            this.keyboard = keyboard;
            yaw = 0f;
            pitch = 0f;
        }
        internal void Update()
        {
            raytracer.Render();
            Input();

            raytracer.camera.lookAtDirection = new Vector3(
            (float)(Math.Sin(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch))),
            (float)Math.Sin(MathHelper.DegreesToRadians(pitch)),
            (float)(Math.Cos(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch)))
        );
        }

        private void Input()
        {
            float moveSpeed = 0.1f;

            if (keyboard[Keys.LeftShift]) moveSpeed = 0.3f;
            else moveSpeed = 0.1f;

            if (keyboard[Keys.W])
            {
                raytracer.camera.position += raytracer.camera.lookAtDirection * moveSpeed;
            }
            if (keyboard[Keys.S])
            {
                raytracer.camera.position -= raytracer.camera.lookAtDirection * moveSpeed;
            }
            if (keyboard[Keys.A])
            {
                raytracer.camera.position -= raytracer.camera.rightDirection * moveSpeed;
            }
            if (keyboard[Keys.D])
            {
                raytracer.camera.position += raytracer.camera.rightDirection * moveSpeed;
            }
            if (keyboard[Keys.E])
            {
                raytracer.camera.position += raytracer.camera.upDirection * moveSpeed;
            }
            if (keyboard[Keys.Q])
            {
                raytracer.camera.position -= raytracer.camera.upDirection * moveSpeed;
            }

            if (keyboard[Keys.Up])
            {
                pitch += 2.5f;
                raytracer.camera.upDirection = Vector3.Normalize(Vector3.Cross(raytracer.camera.rightDirection, raytracer.camera.lookAtDirection));
            }
            if (keyboard[Keys.Down])
            {
                pitch -= 2.5f;
                raytracer.camera.upDirection = Vector3.Normalize(Vector3.Cross(raytracer.camera.rightDirection, raytracer.camera.lookAtDirection));
            }
            if (keyboard[Keys.Left])
            {
                yaw += 2.5f;
                raytracer.camera.rightDirection = Vector3.Normalize(Vector3.Cross(raytracer.camera.lookAtDirection, raytracer.camera.upDirection));
            }
            if (keyboard[Keys.Right])
            {
                yaw -= 2.5f;
                raytracer.camera.rightDirection = Vector3.Normalize(Vector3.Cross(raytracer.camera.lookAtDirection, raytracer.camera.upDirection));
            }

            raytracer.camera.SetScreenPlaneCorners();
        }
    }
}