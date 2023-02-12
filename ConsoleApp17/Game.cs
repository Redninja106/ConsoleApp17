using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17;
public class Game : Simulation
{
    public Scene Scene { get; private set; }

    public Game(string scenePath) : this(SceneLoader.LoadScene(scenePath, Assembly.GetCallingAssembly()))
    {

    }

    public Game(Scene scene)
    {
        this.Scene = scene;
    }

    public override void OnInitialize(AppConfig config)
    {
        Scene.SetActive();
        Scene.Initialize();

        Time.MaxDeltaTime = 1f / 30f;

        DebugWindow.Windows.Add(SceneViewer.Instance);
        DebugWindow.Windows.Add(DebugConsole.Instance);
        DebugWindow.Windows.Add(Inspector.Instance);
        DebugWindow.Windows.Add(CameraWindow.Instance);
    }

    public override void OnRender(ICanvas canvas)
    {
        DebugWindow.LayoutAll();

        canvas.StrokeWidth(0);
        canvas.Clear(Color.Black);

        Camera.Main?.SetDisplaySize(canvas.Width, canvas.Height);
        Camera.Active = Camera.Main;

        Scene.Update();
        Scene.UpdatePhysics();

        if (Camera.Active is not null)
        {
            Camera.Active?.ApplyTo(canvas);

            Scene!.Render(canvas);
        }
        else
        {
            canvas.DrawText("No Active Cameras.", new(5, 5));
        }
    }

    public void NextScene(Scene scene)
    {
        this.Scene = scene;
    }
}
