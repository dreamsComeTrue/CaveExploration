using Godot;
using System;

public class Player : KinematicBody
{
    private const float MOVE_SPEED = 1.1f;

    private SpotLight flashLight;

    public override void _Ready()
    {
        flashLight = GetNode<SpotLight>("FlashLight");
    }

    public override void _Process(float delta)
    {
        Vector3 newTranslation = this.Translation;

        if (Input.IsActionPressed("move_forward"))
        {
            newTranslation.z -= MOVE_SPEED * delta;
        }
        if (Input.IsActionPressed("move_backward"))
        {
            newTranslation.z += MOVE_SPEED * delta;
        }
        if (Input.IsActionPressed("move_left"))
        {
            newTranslation.x -= MOVE_SPEED * delta;
        }
        if (Input.IsActionPressed("move_right"))
        {
            newTranslation.x += MOVE_SPEED * delta;
        }

        if (Input.IsActionJustPressed("action_tool"))
        {
            flashLight.Visible = !flashLight.Visible;
        }

        this.Translation = newTranslation;
    }
}
