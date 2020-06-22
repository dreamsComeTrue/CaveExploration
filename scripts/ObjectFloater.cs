using Godot;
using System;

public class ObjectFloater
{
    private float deltaAccumulator = 0.0f;
    private float floatInitialHeight;
    public float FloatAmplitude = 0.02f;
    public float FloatFrequency = 120.0f;

    public void Initialize(float inputHeight)
    {
        floatInitialHeight = inputHeight;
    }

    public Vector3 CalculateMeshFloat(float delta, Vector3 inputPos)
    {
        deltaAccumulator += delta;

        Vector3 newTranslation = inputPos;

        newTranslation.y = floatInitialHeight;
        newTranslation.y += Mathf.Sin(Mathf.Deg2Rad(deltaAccumulator * (float)Math.PI * FloatFrequency)) * FloatAmplitude;
        
        return newTranslation;
    }
}
