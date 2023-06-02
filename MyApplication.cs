using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace RayTracer
{
    class MyApplication
    {
        // member variables
        public Surface screen;
        public Application application;
        Stopwatch timer = new Stopwatch();

        // constructor
        public MyApplication(Surface screen, KeyboardState keyboard, MouseState mouse)
        {
            this.screen = screen;
            this.application = new Application(screen, keyboard, mouse);

        }
        // initialize
        public void Init()
        {

        }
        // tick: renders one frame
        public void Tick()
        {
            timer.Start();
            application.Update();
            timer.Stop();
            if(timer.ElapsedMilliseconds != 0)
                screen.Print("FPS: " + 1000 / timer.ElapsedMilliseconds, 10, 50, 255 * 256 * 256 + 255 * 256 + 255);
            timer.Restart();
            //screen.Clear(0);
            //screen.Print("hello world", 2, 2, 0xffffff);
            //screen.Line(2, 20, 160, 20, 0xff0000);
        }
    }
}