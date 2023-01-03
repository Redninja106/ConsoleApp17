using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp17.Components.Pong;
internal class ScoreKeeper : Component
{
    public int rightPlayerScore; // 0
    public int leftPlayerScore; // 1

    BallController ballController;
    Goal leftGoal;
    Goal rightGoal;

    public override void Initialize(Entity parent)
    {
        ballController = Scene.Active.FindComponent<BallController>() ?? throw new Exception();
        ballController.initialBallVelocity = GetRandomBallVelocity(1);

        rightGoal = Scene.Active.FindComponent<Goal>(g => g.PlayerIndex == 0) ?? throw new Exception();
        rightGoal.OnScore += RightGoal_OnScore;


        leftGoal = Scene.Active.FindComponent<Goal>(g => g.PlayerIndex == 1) ?? throw new Exception();
        leftGoal.OnScore += LeftGoal_OnScore;
    }

    private void LeftGoal_OnScore()
    {
        rightPlayerScore++;
        ballController.Reset(GetRandomBallVelocity(-1));
    }

    private void RightGoal_OnScore()
    {
        leftPlayerScore++;
        ballController.Reset(GetRandomBallVelocity(1));
    }

    public override void Update()
    {
    }

    public override void Render(ICanvas canvas)
    {
        canvas.ResetState();
        canvas.Translate(canvas.Width / 2f, canvas.Height / 2f);

        canvas.FontStyle(canvas.Height * .075f, FontStyle.Normal);

        var y = canvas.Height * .45f;
        var x = canvas.Width * .25f;

        canvas.DrawText($"{leftPlayerScore}", new(-x, -y));
        canvas.DrawText($"{rightPlayerScore}", new(x, -y));

        base.Render(canvas);
    }

    public override void Layout()
    {
        ImGui.Text($"right score: {rightPlayerScore}");
        ImGui.Text($"left score: {leftPlayerScore}");

        base.Layout();
    }

    private Vector2 GetRandomBallVelocity(int xSign)
    {
        float x = xSign;
        float y = (Random.Shared.NextSingle() - .5f) * 2;

        return new Vector2(x, y).WithLength(5);
    }
}
