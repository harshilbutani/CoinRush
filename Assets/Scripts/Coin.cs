using Fusion;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    [Networked]
    private Vector2 NetworkPosition { get; set; }

    private bool collected;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            NetworkPosition = transform.position;
    }

    public override void Render()
    {
        if (!Object.HasStateAuthority)
            transform.position = NetworkPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Object.HasStateAuthority || collected)
            return;

        NetworkPlayerData playerData = other.GetComponent<NetworkPlayerData>();
        if (playerData == null)
            playerData = other.GetComponentInParent<NetworkPlayerData>();

        if (playerData == null)
            return;

        collected = true;
        CoinManager.Instance?.CollectCoin(playerData);
    }
}
