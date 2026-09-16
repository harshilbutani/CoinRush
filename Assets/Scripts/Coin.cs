using Fusion;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Object.HasStateAuthority)
            return;

        NetworkPlayerData playerData = other.GetComponent<NetworkPlayerData>();
        if (playerData == null)
            playerData = other.GetComponentInParent<NetworkPlayerData>();

        if (playerData != null)
            CoinManager.Instance?.CollectCoin(playerData);
    }

    public void SetVisible(bool visible)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;

        Collider2D coinCollider = GetComponent<Collider2D>();
        if (coinCollider != null)
            coinCollider.enabled = visible;
    }
}
