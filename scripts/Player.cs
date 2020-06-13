using Godot;
using System;

public class Player : KinematicBody
{
	private const float MOVE_SPEED = 1.1f;

	private SpotLight flashLight;
	private MeshInstance mesh;

	private float deltaAccumulator = 0.0f;
	private float floatInitialHeight;
	private float floatAmplitude = 0.02f;
	private float floatFrequency = 120.0f;

	public override void _Ready()
	{
		flashLight = GetNode<SpotLight>("FlashLight");
		mesh = GetNode<MeshInstance>("Mesh");
		floatInitialHeight = mesh.Translation.y;
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

		UpdateMeshFloat(delta);
	}

	private void UpdateMeshFloat(float delta)
	{
		deltaAccumulator += delta;
		
		Vector3 newTranslation = mesh.Translation;

		newTranslation.y = floatInitialHeight;
		newTranslation.y += Mathf.Sin(Mathf.Deg2Rad(deltaAccumulator * (float)Math.PI * floatFrequency)) * floatAmplitude;
		mesh.Translation = newTranslation;
	}
}
