using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RayTracer
{
    class MyApplication
    {
        // member variables
        public Surface screen;
        public Application application;
        // constructor
        public MyApplication(Surface screen, KeyboardState keyboard)
        {
            this.screen = screen;
            this.application = new Application(screen, keyboard);

        }
        // initialize
        public void Init()
        {

        }
        // tick: renders one frame
        public void Tick()
        {
            application.Update();
            //screen.Clear(0);
            //screen.Print("hello world", 2, 2, 0xffffff);
            //screen.Line(2, 20, 160, 20, 0xff0000);
        }
    }
}