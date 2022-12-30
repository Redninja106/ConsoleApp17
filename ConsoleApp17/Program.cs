using ConsoleApp17;
using SimulationFramework;
using SimulationFramework.Drawing;

Scene? scene = null;
Simulation.Create(Init, Render).Run();

void Init(AppConfig config)
{
    scene = SceneLoader.LoadScene("Scenes/test.scene");
    scene.SetActive();
    scene.Initialize();

    Time.MaxDeltaTime = 1f / 30f;

    DebugWindow.Windows.Add(SceneViewer.Instance);
    DebugWindow.Windows.Add(DebugConsole.Instance);
    DebugWindow.Windows.Add(Inspector.Instance);
    DebugWindow.Windows.Add(CameraWindow.Instance);
}

void Render(ICanvas canvas)
{
    DebugWindow.LayoutAll();

    canvas.StrokeWidth(0);
    canvas.Clear(Color.Black);

    if (Camera.Main is not null)
    {
        Camera.Main?.ApplyTo(canvas);

        scene!.Render(canvas);
    }
    else
    {
        canvas.DrawText("No Active Cameras.", new(5, 5));
    }

    scene!.Update();
}