
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
        MouseState mouse;
        static internal float epsilon = 0.0001f;
        private float yaw, pitch;

        internal Application(Surface screen, KeyboardState keyboard, MouseState mouse)
        {
            raytracer = new Raytracer(screen);
            this.keyboard = keyboard;
            this.mouse = mouse;
            yaw = 180f;
            pitch = 0f;
        }
        internal void Update()
        {
            raytracer.Render();

            if (keyboard[Keys.LeftAlt] && keyboard[Keys.RightAlt])
            {

                raytracer.camera.lookAtDirection = new Vector3(0, 0, 1);
                raytracer.camera.rightDirection = new Vector3(-1, 0, 0);
                raytracer.camera.upDirection = new Vector3(0, 1, 0);
                raytracer.camera.SetScreenPlaneCorners();
                raytracer.debugMode = true;
            }

            if (!raytracer.debugMode)
            {
                KeyboardInput();
                MouseInput();


                raytracer.camera.screen.Print(raytracer.camera.fov.ToString(), raytracer.camera.screen.width / 25, raytracer.camera.screen.height / 25, 0xffffff);
            }
        }

        private void KeyboardInput()
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
                if (pitch > 89.0f)
                {
                    pitch = 89.0f;
                }
                raytracer.camera.lookAtDirection = Vector3.Normalize(CalculateFrontDirection());
                raytracer.camera.upDirection = Vector3.Normalize(CalculateUpDirection());
            }
            if (keyboard[Keys.Down])
            {
                pitch -= 2.5f;
                if (pitch < -89.0f)
                {
                    pitch = -89.0f;
                }
                raytracer.camera.lookAtDirection = Vector3.Normalize(CalculateFrontDirection());
                raytracer.camera.upDirection = Vector3.Normalize(CalculateUpDirection());
            }
            if (keyboard[Keys.Left])
            {
                yaw -= 2.5f;
                raytracer.camera.lookAtDirection = Vector3.Normalize(CalculateFrontDirection());
                raytracer.camera.rightDirection = Vector3.Normalize(CalculateRightDirection());
                raytracer.camera.upDirection = Vector3.Normalize(CalculateUpDirection());
            }
            if (keyboard[Keys.Right])
            {
                yaw += 2.5f;
                raytracer.camera.lookAtDirection = Vector3.Normalize(CalculateFrontDirection());
                raytracer.camera.rightDirection = Vector3.Normalize(CalculateRightDirection());
                raytracer.camera.upDirection = Vector3.Normalize(CalculateUpDirection());
            }

            raytracer.camera.SetScreenPlaneCorners();
        }

        private void MouseInput()
        {
            if (mouse.ScrollDelta.Y > 0 && raytracer.camera.fov < 180)
                raytracer.camera.fov += mouse.ScrollDelta.Y * 3f;
            if (mouse.ScrollDelta.Y < 0 && raytracer.camera.fov > 1)
                raytracer.camera.fov += mouse.ScrollDelta.Y * 3f;
            raytracer.camera.fov = Math.Clamp(raytracer.camera.fov, 1, 180);
            raytracer.camera.SetScreenPlaneCorners();
        }

        private Vector3 CalculateFrontDirection()
        {
            float yawRad = MathHelper.DegreesToRadians(yaw);
            float pitchRad = MathHelper.DegreesToRadians(pitch);

            float cosYaw = (float)Math.Cos(yawRad);
            float sinYaw = (float)Math.Sin(yawRad);
            float cosPitch = (float)Math.Cos(pitchRad);
            float sinPitch = (float)Math.Sin(pitchRad);

            return new Vector3(
                sinYaw * cosPitch,
                sinPitch,
                -cosYaw * cosPitch
            );
        }

        private Vector3 CalculateRightDirection()
        {
            float yawRad = MathHelper.DegreesToRadians(yaw);

            float cosYaw = (float)Math.Cos(yawRad);
            float sinYaw = (float)Math.Sin(yawRad);

            return new Vector3(
                cosYaw,
                0f,
                sinYaw
            );
        }

        private Vector3 CalculateUpDirection()
        {
            return Vector3.Cross(CalculateRightDirection(), CalculateFrontDirection());
        }
    }
}