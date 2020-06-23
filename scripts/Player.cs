using Godot;
using System;

public class Player : KinematicBody
{
    [Export]
    public float MOVE_SPEED = 1.3f;

    private SpotLight flashLight;
    private MeshInstance mesh;

    private Vector3 movementDirection = Vector3.Zero;
    private Vector3 lastDirection = Vector3.Zero;

    private float angleAccumulator = 0.0f;

    private ObjectFloater objectFloater;

    public override void _Ready()
    {
        flashLight = GetNode<SpotLight>("FlashLight");
        mesh = GetNode<MeshInstance>("Mesh");
        objectFloater = new ObjectFloater();
        objectFloater.Initialize(mesh.Translation.y);
    }

    public override void _Process(float delta)
    {
        movementDirection = Vector3.Zero;

        _HandleKeyInputs();

        this.MoveAndCollide(movementDirection.Normalized() * delta * MOVE_SPEED);

        _UpdateMeshFloat(delta);
        _FaceMeshToDirection(delta);
    }

    private bool inMovement = false;
    private void _HandleKeyInputs()
    {
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

        if (Input.IsActionJustPressed("action_tool"))
        {
            flashLight.Visible = !flashLight.Visible;
        }
    }

    private void _UpdateMeshFloat(float delta)
    {
        float floatFrequency = objectFloater.FloatFrequency;

        if (inMovement)
        {
            floatFrequency *= 1.5f;
        }

        mesh.Translation = objectFloater.CalculateMeshFloat(delta, mesh.Translation, floatFrequency);
    }

    private void _FaceMeshToDirection(float delta)
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
