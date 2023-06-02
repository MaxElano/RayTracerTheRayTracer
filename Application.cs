
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
        Camera camera;
        Primitive target;
        static internal float epsilon = 0.0001f;
        private int targetCounter;
        private bool targetSwitched;

        internal Application(Surface screen, KeyboardState keyboard, MouseState mouse)
        {
            raytracer = new Raytracer(screen);
            this.keyboard = keyboard;
            this.mouse = mouse;
            camera = raytracer.camera;
            camera.yaw = 180f;
            camera.pitch = 0f;
            targetCounter = 0;
            target = raytracer.scene.primitives[targetCounter];
        }
        internal void Update()
        {
            raytracer.Render();

            if (keyboard[Keys.LeftAlt] && keyboard[Keys.RightAlt])
            {
                camera.lookAtDirection = new Vector3(0, 0, 1);
                camera.rightDirection = new Vector3(-1, 0, 0);
                camera.upDirection = new Vector3(0, 1, 0);
                camera.SetScreenPlaneCorners();
                raytracer.debugMode = true;
            }

            if (!raytracer.debugMode)
            {
                KeyboardInput();
                MouseInput();

                camera.fov = camera.CalculateFOV(camera.resolution, camera.distanceToScreenPlane);
                camera.screen.Print(((int)camera.fov).ToString(), camera.screen.width / 25, camera.screen.height / 25, 0xffffff);
            }
            else
            {
                KeyboardInputDebugger();
            }
        }

        private void KeyboardInputDebugger()
        {
            if (keyboard[Keys.P])
            {
                raytracer.showPrimaryRays = !raytracer.showPrimaryRays;
            }
        }

        private void KeyboardInput()
        {
            float moveSpeed = 0.1f;

            if (keyboard[Keys.LeftShift]) moveSpeed = 0.3f;
            else moveSpeed = 0.1f;

            if (keyboard[Keys.W])
            {
                camera.position += camera.lookAtDirection * moveSpeed;
            }
            if (keyboard[Keys.S])
            {
                camera.position -= camera.lookAtDirection * moveSpeed;
            }
            if (keyboard[Keys.A])
            {
                camera.position -= camera.rightDirection * moveSpeed;
            }
            if (keyboard[Keys.D])
            {
                camera.position += camera.rightDirection * moveSpeed;
            }
            if (keyboard[Keys.E])
            {
                camera.position += camera.upDirection * moveSpeed;
            }
            if (keyboard[Keys.Q])
            {
                camera.position -= camera.upDirection * moveSpeed;
            }

            if (keyboard[Keys.Up])
            {
                camera.pitch += 25f * moveSpeed;
                if (camera.pitch > 89.0f)
                {
                    camera.pitch = 89.0f;
                }
                camera.SetFrontDirection();
                camera.SetUpDirection();
            }
            if (keyboard[Keys.Down])
            {
                camera.pitch -= 25f * moveSpeed;
                if (camera.pitch < -89.0f)
                {
                    camera.pitch = -89.0f;
                }
                camera.SetFrontDirection();
                camera.SetUpDirection();
            }
            if (keyboard[Keys.Left])
            {
                camera.yaw -= 25f * moveSpeed;
                camera.SetFrontDirection();
                camera.SetRightDirection();
                camera.SetUpDirection();
            }
            if (keyboard[Keys.Right])
            {
                camera.yaw += 25f * moveSpeed;
                camera.SetFrontDirection();
                camera.SetRightDirection();
                camera.SetUpDirection();
            }

            if (keyboard[Keys.P] && !targetSwitched)
            {
                targetSwitched = true;
                if (targetCounter < raytracer.scene.primitives.Count - 1)
                    targetCounter++;
                else
                    targetCounter = 0;
                
                target = raytracer.scene.primitives[targetCounter]; 
                camera.LookAt(target);
            }
            else if (keyboard[Keys.O] && !targetSwitched)
            {
                targetSwitched = true;
                if (targetCounter > 0)
                    targetCounter--;
                else
                    targetCounter = raytracer.scene.primitives.Count - 1;

                target = raytracer.scene.primitives[targetCounter];
                camera.LookAt(target);
            }

            if (keyboard[Keys.Space])
            {
                camera.LookAt(target);
            }

            if (!keyboard[Keys.P] && !keyboard[Keys.O] && targetSwitched)
                targetSwitched = false;

            camera.SetScreenPlaneCorners();
        }

        private void MouseInput()
        {
            if (mouse.ScrollDelta.Y > 0 && camera.fov > 2)
                camera.SetFOV(camera.resolution, camera.fov-=3);
            if (mouse.ScrollDelta.Y < 0 && camera.fov < 88)
                camera.SetFOV(camera.resolution, camera.fov+=3);

            camera.SetScreenPlaneCorners();
        }
    }
}