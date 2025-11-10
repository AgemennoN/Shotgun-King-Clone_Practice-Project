using System;
using System.Collections;
using UnityEngine;

public class Pawn : EnemyPiece {
    public static event Action<Pawn, EnemyType> OnPawnPromoted;
    public static float PromotionDuration = 0.5f;

    public override void TakeAction() {
        base.TakeAction();

        CheckPromote();
    }

    private void CheckPromote() {
        if (currentTile.GridPosition.y == 0) {
            TurnManager.Instance.RegisterAction(HandlePromotion());
        }
    }

    private IEnumerator HandlePromotion() {
        yield return new WaitForSeconds(movementDuration);


        GetTile().SetPiece(null);

        EnemyType newType = EnemyType.Pawn;

        float randomValue = UnityEngine.Random.value; // Random float between 0.0 and 1.0
        if (randomValue < 0.15f)
            newType = EnemyType.Queen;
        else if (randomValue < 0.15f + 0.25f)
            newType = EnemyType.Rook;
        else if (randomValue < 0.15f + 0.25f + 0.30f)
            newType = EnemyType.Bishop;
        else
            newType = EnemyType.Knight;

        OnPawnPromoted?.Invoke(this, newType);

        visualEffects.SpriteFadeOutAnimation(Pawn.PromotionDuration, true);
        StartCoroutine(DestroyedIn(Pawn.PromotionDuration));
    }

}
