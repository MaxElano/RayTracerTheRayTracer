
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


        internal Application(Surface screen, KeyboardState keyboard)
        {
            raytracer = new Raytracer(screen);
            this.keyboard = keyboard;
        }
        internal void Update()
        {
            raytracer.Render();
            Input();
        }

        private void Input()
        {
            float moveSpeed = 0.1f;

            if (keyboard[Keys.LeftShift]) moveSpeed = 2.5f;
            else moveSpeed = 1;

            if (keyboard[Keys.W])
            {
                raytracer.camera.position += raytracer.camera.lookAtDirection * 0.1f;
                raytracer.camera.SetScreenPlaneCorners();
            }
            if (keyboard[Keys.S])
            {
                raytracer.camera.position -= raytracer.camera.lookAtDirection * 0.1f;
                raytracer.camera.SetScreenPlaneCorners();
            }
            if (keyboard[Keys.A])
            {
                raytracer.camera.position -= raytracer.camera.rightDirection * 0.1f;
                raytracer.camera.SetScreenPlaneCorners();
            }
            if (keyboard[Keys.D])
            {
                raytracer.camera.position += raytracer.camera.rightDirection * 0.1f;
                raytracer.camera.SetScreenPlaneCorners();
            }
            if (keyboard[Keys.E]) VerticalMovement(0.1f, moveSpeed);
            if (keyboard[Keys.Q]) VerticalMovement(-0.1f, moveSpeed);

            if (keyboard[Keys.Up]) LookDirection(0, 0.05f);
            if (keyboard[Keys.Down]) LookDirection(0, -0.05f);
            if (keyboard[Keys.Left]) LookDirection(0.05f, 0);
            if (keyboard[Keys.Right]) LookDirection(-0.05f, 0);
        }

        private void VerticalMovement(float movement, float moveSpeed)
        {
            raytracer.camera.position.Y += movement;
            raytracer.camera.SetScreenPlaneCorners();
        }

        private void LookDirection(float HoriAxis, float VertAxis)
        {
            var camera = raytracer.camera;

            if (HoriAxis != 0)
            {
                if (camera.lookAtDirection.X >= 0 && camera.lookAtDirection.Z > 0)
                {
                    camera.lookAtDirection.X += HoriAxis;
                    camera.lookAtDirection.Z -= HoriAxis;
                    camera.rightDirection.X += HoriAxis;
                    camera.rightDirection.Z += HoriAxis;
                }
                if (camera.lookAtDirection.X > 0 && camera.lookAtDirection.Z <= 0)
                {
                    camera.lookAtDirection.X -= HoriAxis;
                    camera.lookAtDirection.Z -= HoriAxis;
                    camera.rightDirection.X += HoriAxis;
                    camera.rightDirection.Z -= HoriAxis;
                }
                if (camera.lookAtDirection.X <= 0 && camera.lookAtDirection.Z < 0)
                {
                    camera.lookAtDirection.X -= HoriAxis;
                    camera.lookAtDirection.Z += HoriAxis;
                    camera.rightDirection.X -= HoriAxis;
                    camera.rightDirection.Z -= HoriAxis;
                }
                if (camera.lookAtDirection.X < 0 && camera.lookAtDirection.Z >= 0)
                {
                    camera.lookAtDirection.X += HoriAxis;
                    camera.lookAtDirection.Z += HoriAxis;
                    camera.rightDirection.X -= HoriAxis;
                    camera.rightDirection.Z += HoriAxis;
                }
                camera.lookAtDirection.Normalize();
                if (camera.lookAtDirection.Y != 0) 
                    camera.FixUpDirection();
                else
                    camera.FixRightDirection();
            }

            if (VertAxis != 0)
            {
                if (camera.lookAtDirection.Y < 1 && camera.lookAtDirection.Y > -1)
                    camera.lookAtDirection.Y += VertAxis;
                camera.lookAtDirection.Normalize();
                camera.FixUpDirection();
            }

            camera.SetScreenPlaneCorners();
        }
    }
}
