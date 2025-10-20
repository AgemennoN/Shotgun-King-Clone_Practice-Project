using Unity.VisualScripting;
using UnityEngine;

public class BoardTile : MonoBehaviour {
    
    public static BoardTile Create(float tileSize, int positionX, int positionY, Sprite sprite, ChessPiece chessPiece=null, GameObject parent=null) {
        GameObject tileObject = new GameObject($"Tile_{positionX}_{positionY}");
        BoardTile tile = tileObject.AddComponent<BoardTile>();
        tile.positionX = positionX;
        tile.positionY = positionY;
        tile.tileSize = tileSize;

        tile.CreateSprite(sprite);
        tile.CreateCollider();

        tile.SetPosition();
        tile.SetPiece(chessPiece);
        if (chessPiece != null) {
            chessPiece.SetPosition(tile);
        }
        if (parent != null) {
            tile.transform.SetParent(parent.transform);
        }

        return tile;
    }

    private int positionX;
    private int positionY;
    private float tileSize;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private PlayerManager playerManager;
    public ChessPiece pieceOnIt; // TO DO: Should it be public? Or is there a better option to be more OOP

    private void Start() {
        playerManager = PlayerManager.Instance;
    }

    public ChessPiece GetPiece() => pieceOnIt;

        public void SetPiece(ChessPiece piece) {
        pieceOnIt = piece;
    }

    private void SetPosition() {
        // To do (minor): instead of using a constant as -4 get board size and use -BoardSize/2
        float worldPositionX = (-4 + positionX) * tileSize + tileSize/2;
        float worldPositionY = (-4 + positionY) * tileSize + tileSize/2;
        transform.position = new Vector3(worldPositionX, worldPositionY, 0);
    }

    public Vector2Int GetPosition() {
        return new Vector2Int(positionX, positionY);
    }

    void OnMouseDown() {
        if (TurnManager.Instance.IsPlayerTurn() && playerManager.IsActionAvailable()) {
            playerManager.OnTileClicked(this);
            HighlightTile(false);
        }
    }

    void OnMouseEnter() {
        if (TurnManager.Instance.IsPlayerTurn()) {
            if (playerManager.playerAvailableMoves.Contains(this)){
                HighlightTile(true); // PlaceHolder visual effect
            }
        }
    }
    void OnMouseExit() {
        if (TurnManager.Instance.IsPlayerTurn()) {
            HighlightTile(false);
        }
    }

    private void HighlightTile(bool highlight) {
        spriteRenderer.color = highlight ? Color.yellow : Color.white;
        // TO DO: Should be an arrow from playerpiece to the tile's direction to indicate movement
    }

    private void CreateSprite(Sprite sprite) {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "ChessBoard";
        transform.localScale = new Vector3(4.5f, 4.5f);
    }

    private void CreateCollider() {
        boxCollider = gameObject.AddComponent<BoxCollider2D>();

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        Vector2 worldSize = new Vector2(spriteSize.x * transform.localScale.x, spriteSize.y * transform.localScale.y);
        boxCollider.size = spriteSize;
        boxCollider.offset = spriteRenderer.sprite.bounds.center;
    }

}
