using System;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public NetworkList<int> strokes;

    void Awake() => strokes = new NetworkList<int>();

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            strokes.Clear();
            foreach (var p in NetworkManager.Singleton.ConnectedClientsList)
                strokes.Add(0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddStrokeServerRpc(ulong playerId)
    {
        var idx = IndexOf(playerId);
        if (idx >= 0) strokes[idx] = strokes[idx] + 1;
    }

    int IndexOf(ulong playerId)
    {
        var list = NetworkManager.Singleton.ConnectedClientsList;
        for(int i = 0; i < list.Count; i++)
        {
            if (list[i].ClientId == playerId) return i;
        }

        return -1;
    }
}
