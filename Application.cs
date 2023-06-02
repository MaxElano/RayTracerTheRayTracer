
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Diagnostics;

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
        private bool targetSwitched, debugModeSwitch
        Stopwatch pressTimer = new Stopwatch();
        bool showHelp = true;
        Surface screen;
        int textInterval = 18;
        int pressInterval = 600;
        int distanceCounter = 1;

        internal Application(Surface screen, KeyboardState keyboard, MouseState mouse)
        {
            this.screen = screen;
            pressTimer.Start();
            raytracer = new Raytracer(screen);
            this.keyboard = keyboard;
            this.mouse = mouse;
            camera = raytracer.camera;
            camera.yaw = 180f;
            camera.pitch = 0f;
            targetCounter = 0;
            target = raytracer.scene.primitives[targetCounter];
        }

        //Function that runs every tick, renders the raytracer, checks the keyboard and mouse input and prints the current FOV on the screen
        internal void Update()
        {
            raytracer.Render();

            pressTimer.Stop();
            if (keyboard[Keys.H] && pressTimer.ElapsedMilliseconds > pressInterval)
            {
                showHelp = !showHelp;
                pressTimer.Reset();
            }
            pressTimer.Start();
            if (showHelp)
                ShowHelp();


            if (!raytracer.debugMode)
            {
                KeyboardInput();
                MouseInput();

                camera.fov = camera.CalculateFOV();
                camera.screen.Print("FOV: " + ((int)camera.fov).ToString(), screen.width - 100, 28, 0xffffff);
            }
            else
            {
                KeyboardInputDebugger();
            }
        }

        //Shows the controls on the side of the screen
        private void ShowHelp()
        {
            distanceCounter = 1;
            if (raytracer.debugMode)
            {
                screen.Print("Turn On/Off", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("H : Help Menu", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("P : Primary Rays", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("S : Shadow Rays", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("R : Refraction Rays", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("L : Reflaction Rays", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("I : Primitives", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Exit Debug: Lalt", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
            }
            else
            {
                screen.Print("Movement Controls", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Move: W/A/S/D", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Up: E", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Down: Q", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Look Around: Up/Left/Down/Right", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Next Primitive: P", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Previous Primitive: O", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Look At: Space", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
                distanceCounter++;
                screen.Print("Enter Debug: Lalt", 10, distanceCounter * textInterval, 255 * 256 * 256 + 255 * 256 + 255);
            }
        }

        private void KeyboardInputDebugger()
        {
            pressTimer.Stop();
            if (keyboard[Keys.P] && pressTimer.ElapsedMilliseconds > pressInterval)
            {
                raytracer.showPrimaryRays = !raytracer.showPrimaryRays;
                pressTimer.Reset();
            }
            if (keyboard[Keys.S] && pressTimer.ElapsedMilliseconds > pressInterval)
            {
                raytracer.showShadowRays = !raytracer.showShadowRays;
                pressTimer.Reset();
            }
            if (keyboard[Keys.R] && pressTimer.ElapsedMilliseconds > pressInterval)
            {
                raytracer.showRefractionRays = !raytracer.showRefractionRays;
                pressTimer.Reset();
            }
            if (keyboard[Keys.L] && pressTimer.ElapsedMilliseconds > pressInterval)
            {
                raytracer.showReflectionRays = !raytracer.showReflectionRays;
                pressTimer.Reset();
            }
            if (keyboard[Keys.I] && pressTimer.ElapsedMilliseconds > pressInterval)
            {
                raytracer.showPrimitives = !raytracer.showPrimitives;
                pressTimer.Reset();
            }
            pressTimer.Start();
            //Pressing leftAlt in debugMode allows you to switch back to normal view
            if (keyboard[Keys.LeftAlt] && raytracer.debugMode)
            {
                raytracer.debugMode = false;
                debugModeSwitch = true;
            }
        }

        //Checks the users keyboard inputs to control speed, movement, rotation, cycling through primitives and focusing on current targeted primitive
        private void KeyboardInput()
        {
            float moveSpeed = 0.1f;

            //Speeds up the movement and rotation of camera when holding down leftShift
            if (keyboard[Keys.LeftShift]) moveSpeed = 0.3f;
            else moveSpeed = 0.1f;

            //Controls the cameras position according to key input (WASDEQ)
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

            //Controls the cameras rotation according to key input (Arrow Keys)
            if (keyboard[Keys.Up])
            {
                camera.pitch += 25f * moveSpeed;
                if (camera.pitch > 89.0f)
                {
                    camera.pitch = 89.0f;
                }
                camera.SetLookAtDirection();
                camera.SetUpDirection();
            }
            if (keyboard[Keys.Down])
            {
                camera.pitch -= 25f * moveSpeed;
                if (camera.pitch < -89.0f)
                {
                    camera.pitch = -89.0f;
                }
                camera.SetLookAtDirection();
                camera.SetUpDirection();
            }
            if (keyboard[Keys.Left])
            {
                camera.yaw -= 25f * moveSpeed;
                camera.SetLookAtDirection();
                camera.SetRightDirection();
                camera.SetUpDirection();
            }
            if (keyboard[Keys.Right])
            {
                camera.yaw += 25f * moveSpeed;
                camera.SetLookAtDirection();
                camera.SetRightDirection();
                camera.SetUpDirection();
            }

            //Cycles through the list of primitives in scene, key input P cycles to the right and key input O cycles to the left through the list of primitives. This also immediately looks at the given primitive
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

            //When Space is held down it targets the current target primitive so long as it stays held down
            if (keyboard[Keys.Space])
            {
                camera.LookAt(target);
            }

            //Makes sure that you need to let go of P or O before being able to switch again
            if (!keyboard[Keys.P] && !keyboard[Keys.O] && targetSwitched)
                targetSwitched = false;

            //Makes sure that you need to let go of LeftAlt before being able to switch back to debugMode
            if (!keyboard[Keys.LeftAlt])
                debugModeSwitch = false;

            //If both Alt keys are held down, the screen enters debug mode
            if (keyboard[Keys.LeftAlt]&& !raytracer.debugMode && !debugModeSwitch)
            {
                camera.lookAtDirection = new Vector3(0, 0, 1);
                camera.CalculateNewPitchYaw();
                camera.SetLookAtDirection();
                camera.SetRightDirection();
                camera.SetUpDirection();
                camera.SetScreenPlaneCorners();
                raytracer.debugMode = true;
            }

            camera.SetScreenPlaneCorners();
        }

        //Checks the users mouse input for scrolling for FOV control
        private void MouseInput()
        {
            if (mouse.ScrollDelta.Y > 0 && camera.fov > 2)
                camera.SetFOV(camera.fov-=3);
            if (mouse.ScrollDelta.Y < 0 && camera.fov < 88)
                camera.SetFOV(camera.fov+=3);

            camera.SetScreenPlaneCorners();
        }
    }
}