using System.Collections.Generic;
using Godot;

public class GameManager : Node
{
    public static float DEFAULT_LEVEL_TIME = 3 * 60.0f;
    private Signals signals;

    public const float timeThresholdWarning = 10.0f;

    public float timeLeft = DEFAULT_LEVEL_TIME;
    private float timeAccumulator = 0.0f;

    public string PlayerName;

    public override void _Ready()
    {
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.MapGenerated), this, nameof(OnMapGenerated));
    }

    private void OnMapGenerated()
    {
        ResetGameTime();
    }

    public override void _Process(float delta)
    {
        timeAccumulator += delta;

        if (timeLeft <= timeThresholdWarning)
        {
            if (timeAccumulator >= 0.05f)
            {
                timeAccumulator = 0.0f;
                timeLeft -= 0.05f;
                signals.EmitSignal(nameof(Signals.PulseGameplayTimer), timeLeft);
            }
        }
        else if (timeAccumulator >= 1.0f)
        {
            timeAccumulator = 0.0f;
            timeLeft -= 1.0f;
            signals.EmitSignal(nameof(Signals.PulseGameplayTimer), timeLeft);
        }
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if ((KeyList)@event.Scancode == KeyList.F12 && @event.IsPressed())
        {
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
    }

    public void ResetGameTime()
    {
        timeAccumulator = 0.0f;
        timeLeft = DEFAULT_LEVEL_TIME;
    }

    public static Dictionary<int, Player> players = new Dictionary<int, Player>();

    // public void SpawnPlayer(int id, string userName, Vector3 position, Quaternion rotation)
    // {
    //     GameObject player;

    //     if (id == NetworkClient.Instance.id)
    //     {
    //         player = Instantiate(localPlayerPrefab, position, rotation);
    //     }
    //     else
    //     {
    //         Debug.Log($"Spawn at: {position}");
    //         player = Instantiate(playerPrefab, position, rotation);
    //     }

    //     PlayerManager playerManager = player.GetComponent<PlayerManager>();

    //     playerManager.id = id;
    //     playerManager.userName = userName;
    //     playerManager.characterController = player.GetComponent<CharacterController>();
    //     playerManager.playerMesh = player.transform.GetChild(0).gameObject;
    //     playerManager.playerObject = player;

    //     players.Add(id, playerManager);
    // }
}
