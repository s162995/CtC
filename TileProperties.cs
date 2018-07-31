using UnityEngine;

public enum Item
{
    None,
    Floor,
    Wall,
    Pit,
    Room,
    Agent
};

// The different properties of a Tile in the game.
public class TileProperties : MonoBehaviour {

    public Sprite[] sprites;
    public Item Type {
        get { return type; }
        set {
            type = value;

            switch (value) {
                case Item.Wall:
                    spriteRenderer.sprite = sprites[1];
                    spriteRenderer.color = shadow;
                    break;
                case Item.Floor:
                    spriteRenderer.sprite = sprites[0];
                    spriteRenderer.color = shadow;
                    break;
                case Item.Pit:
                    spriteRenderer.sprite = sprites[2];
                    break;
                default:
                    break;
            }
        }
    }
    public Color shadow = Color.gray;

    private Item type;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetTileColor(Color c)
    {
        spriteRenderer.color = new Color(c.r, c.g, c.b, c.a);
    }

    public Color GetTileColor()
    {
        return spriteRenderer.color;
    }
}