using Godot;
using System;

public class Player : KinematicBody
{
    private const float MOVE_SPEED = 1.1f;

    private SpotLight flashLight;
    private MeshInstance mesh;

    private Vector3 movementDirection = Vector3.Zero;
    private Vector3 lastDirection = Vector3.Zero;

    private float angleAccumulator = 0.0f;

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
        movementDirection = Vector3.Zero;

        if (Input.IsActionPressed("move_forward"))
        {
            movementDirection.z -= 1.0f;
        }
        if (Input.IsActionPressed("move_backward"))
        {
            movementDirection.z += 1.0f;
        }
        if (Input.IsActionPressed("move_left"))
        {
            movementDirection.x -= 1.0f;
        }
        if (Input.IsActionPressed("move_right"))
        {
            movementDirection.x += 1.0f;
        }

        if (Input.IsActionJustPressed("action_tool"))
        {
            flashLight.Visible = !flashLight.Visible;
        }

        this.Translation += movementDirection.Normalized() * delta;

        _UpdateMeshFloat(delta);
        _FaceMeshToDirection(delta);
    }

    private void _UpdateMeshFloat(float delta)
    {
        deltaAccumulator += delta;

        Vector3 newTranslation = mesh.Translation;

        newTranslation.y = floatInitialHeight;
        newTranslation.y += Mathf.Sin(Mathf.Deg2Rad(deltaAccumulator * (float)Math.PI * floatFrequency)) * floatAmplitude;
        mesh.Translation = newTranslation;
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

            if (Mathf.IsEqualApprox(currentRotationDegress, targetAngleDegress, 0.1f) || angleAccumulator > 1.0f
            || lastDirection != directionNormalized)
            {
                angleAccumulator = 0.0f;
            }
            else
            {
                float angle = Mathf.LerpAngle(mesh.Rotation.y, targetAngle, angleAccumulator);

                angleAccumulator += delta * 0.4f;
                mesh.Rotation = new Vector3(0.0f, angle, 0.0f);
            }

            // GD.Print("A > " + currentRotationDegress);
            // GD.Print("B > " + targetAngleDegress);
            // GD.Print("C > " + angleAccumulator);

            lastDirection = directionNormalized;
        }
    }
}
