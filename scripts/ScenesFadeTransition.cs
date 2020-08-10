using Godot;
using System;

public class ScenesFadeTransition : CanvasLayer
{
    public String newScene;

    private Node currentScene;

    public void Init()
    {
        currentScene = GetTree().Root.GetChild(GetTree().Root.GetChildCount() - 1);
    }

    public void ChangeScenes()
    {
        if (currentScene != null)
        {
            currentScene.QueueFree();
        }

        GetTree().ChangeScene(newScene);
    }

    public void Run(string newScenePath)
    {
        newScene = newScenePath;

        GetNode<AnimationPlayer>("AnimationPlayer").Play("fade");
    }

}
