using System.Collections.Generic;
using Godot;

public class Player : KinematicBody
{
	[Export]
	public float MOVE_SPEED = 1.3f;
	public float RUN_SPEED = 3.0f;

	private MeshInstance mesh;

	private Particles footStepsParticles;

	private Vector3 movementDirection = Vector3.Zero;
	private Vector3 lastDirection = Vector3.Zero;

	private float angleAccumulator = 0.0f;

	private bool inputEnabled = true;

	private ObjectFloater objectFloater;

	private Signals signals;

	public override void _Ready()
	{
		signals = (Signals)GetNode("/root/Signals");
		mesh = GetNode<MeshInstance>("Mesh");
		footStepsParticles = GetNode<Particles>("FootStepsParticles");
		objectFloater = new ObjectFloater();
		objectFloater.Initialize(mesh.Translation.y);

		signals.Connect(nameof(Signals.InGameMenuVisibilityChanged), this, nameof(OnInGameMenuVisibilityChanged));
		signals.Connect(nameof(Signals.MapGenerated), this, nameof(OnMapGenerated));
	}

	private void OnMapGenerated()
	{
		CaveGeneratorNode caveGenerator = GetParent().GetNode<CaveGeneratorNode>("CaveGenerator");
		List<CaveGenerator.Room> rooms = caveGenerator.rooms;
		CaveGenerator.Room room = rooms[(int)GD.RandRange(0, rooms.Count)];
		Vector2 middlePoint = (room.Area.Position + room.Area.Size / 2) * 0.5f;

		Vector3 newTranslation = new Vector3(middlePoint.x, Translation.y, middlePoint.y);
		Translation = newTranslation;

		signals.EmitSignal(nameof(Signals.PlayerMoved), this.Translation);
	}

	public override void _Process(float delta)
	{
		movementDirection = Vector3.Zero;

		_HandleKeyInputs();

		float moveSpeed = MOVE_SPEED;
		if (Input.IsKeyPressed((int)KeyList.Shift))
		{
			moveSpeed *= RUN_SPEED;
		}

		this.MoveAndCollide(movementDirection.Normalized() * delta * moveSpeed);

		if (!movementDirection.IsEqualApprox(Vector3.Zero))
		{
			signals.EmitSignal(nameof(Signals.PlayerMoved), this.Translation);
			footStepsParticles.Emitting = true;
		}
		else
		{
			footStepsParticles.Emitting = false;
		}

		UpdateMeshFloat(delta);
		FaceMeshToDirection(delta);
	}

	private bool inMovement = false;
	private void _HandleKeyInputs()
	{
		if (!inputEnabled)
		{
			return;
		}

		inMovement = false;

		if (Input.IsActionPressed("move_forward"))
		{
			movementDirection.z -= 1.0f;
			inMovement = true;
		}
		if (Input.IsActionPressed("move_backward"))
		{
			movementDirection.z += 1.0f;
			inMovement = true;
		}
		if (Input.IsActionPressed("move_left"))
		{
			movementDirection.x -= 1.0f;
			inMovement = true;
		}
		if (Input.IsActionPressed("move_right"))
		{
			movementDirection.x += 1.0f;
			inMovement = true;
		}
	}

	private void OnInGameMenuVisibilityChanged(bool visible)
	{
		inputEnabled = !visible;
	}

	private void UpdateMeshFloat(float delta)
	{
		float floatFrequency = objectFloater.FloatFrequency;

		if (inMovement)
		{
			floatFrequency *= 1.5f;
		}

		mesh.Translation = objectFloater.CalculateMeshFloat(delta, mesh.Translation, floatFrequency);
	}

	private void FaceMeshToDirection(float delta)
	{
		Vector3 directionNormalized = movementDirection.Normalized();

		if (directionNormalized.Length() >= 0.001f)
		{
			float currentRotationDegress = (mesh.RotationDegrees.y % 360.0f);
			float targetAngle = Mathf.Atan2(directionNormalized.x, directionNormalized.z);
			float targetAngleDegress = (Mathf.Rad2Deg(targetAngle) % 360.0f);

			if (targetAngleDegress < 0)
			{
				targetAngleDegress = 360.0f + targetAngleDegress;
			}

			if (Mathf.IsEqualApprox(currentRotationDegress, targetAngleDegress, 0.1f) || lastDirection != directionNormalized)
			{
				angleAccumulator = 0.0f;
			}
			else
			{
				float angle = Mathf.LerpAngle(mesh.Rotation.y, targetAngle, angleAccumulator);
				mesh.Rotation = new Vector3(0.0f, angle, 0.0f);

				angleAccumulator += delta * 0.4f;
			}

			lastDirection = directionNormalized;
		}
	}
}
