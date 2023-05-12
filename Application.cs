
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

            if (keyboard[Keys.W]) Movement(0, 0, 0.1f, moveSpeed);
            if (keyboard[Keys.S]) Movement(0, 0, -0.1f, moveSpeed);
            if (keyboard[Keys.A]) Movement(0.1f, 0, 0, moveSpeed);
            if (keyboard[Keys.D]) Movement(-0.1f, 0, 0, moveSpeed);
            if (keyboard[Keys.E]) Movement(0, 0.1f, 0, moveSpeed);
            if (keyboard[Keys.Q]) Movement(0, -0.1f, 0, moveSpeed);

            if (keyboard[Keys.Up]) LookDirection(0, 0.1f, 0);
            if (keyboard[Keys.Down]) LookDirection(0, -0.1f, 0);
            if (keyboard[Keys.Left]) LookDirection(0.1f, 0, 0);
            if (keyboard[Keys.Right]) LookDirection(-0.1f, 0, 0);
        }

        private void Movement(float x, float y, float z, float moveSpeed)
        {
            Vector3 change = new Vector3(x, y, z) * moveSpeed;
            raytracer.camera.position += change;
            raytracer.camera.leftTop += change;
            raytracer.camera.rightTop += change;
            raytracer.camera.leftBottom += change;
            raytracer.camera.rightBottom += change;
        }

        private void LookDirection(float  x, float y, float z)
        {
            Vector3 change = new Vector3(x, y, z);
            raytracer.camera.lookAtDirection += change;
            raytracer.camera.leftTop += change;
            raytracer.camera.rightTop += change;
            raytracer.camera.leftBottom += change;
            raytracer.camera.rightBottom += change;
        }
    }
}
