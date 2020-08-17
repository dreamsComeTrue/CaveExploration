using Godot;

public class MinimapCanvas : Control
{
    private Signals signals;
    private CaveGenerator.CellType[,] mapData;
    private Vector3 playerPosition;

    public override void _Ready()
    {
        signals = (Signals)GetNode("/root/Signals");
        signals.Connect(nameof(Signals.MapGenerated), this, nameof(OnMapGenerated));
        signals.Connect(nameof(Signals.PlayerMoved), this, nameof(OnPlayerMoved));
    }

    private void OnMapGenerated()
    {
        CaveGeneratorNode generator = GetTree().Root.GetNode<CaveGeneratorNode>("Gameplay/ViewportContainer/Viewport/CaveGenerator");

        mapData = generator.mapData;
        Update();
    }

    private void OnPlayerMoved(Vector3 newPosition)
    {
        playerPosition = newPosition;
        Update();
    }

    public override void _Draw()
    {
        if (mapData == null)
        {
            return;
        }

        Rect2 wholeRect = new Rect2(0, 0, RectSize);
        DrawRect(wholeRect, new Color(0.1f, 0.01f, 0.2f));

        Vector2 scaler = new Vector2(this.RectSize.x / mapData.GetUpperBound(0), this.RectSize.y / mapData.GetUpperBound(1));

        for (int y = mapData.GetLowerBound(1); y < mapData.GetUpperBound(1); y++)
        {
            for (int x = mapData.GetLowerBound(0); x < mapData.GetUpperBound(0); x++)
            {
                Vector2 position = new Vector2(x * scaler.x, y * scaler.y);
                Rect2 rect = new Rect2(position, scaler);

                if (mapData[x, y] == CaveGenerator.CellType.Room)
                {
                    DrawRect(rect, Colors.RoyalBlue);
                }
                else if (mapData[x, y] == CaveGenerator.CellType.Treasure)
                {
                    DrawRect(rect, Colors.Yellow);
                }
                else if (mapData[x, y] == CaveGenerator.CellType.Decoration)
                {
                    DrawRect(rect, Colors.LightSkyBlue);
                }
            }
        }

        Rect2 playerRect = new Rect2(new Vector2(playerPosition.x, playerPosition.z) * scaler * 2, scaler);
        DrawRect(playerRect, Colors.Fuchsia);
    }
}
