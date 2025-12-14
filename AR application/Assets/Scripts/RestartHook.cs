using UnityEngine;

public class RestartHook : MonoBehaviour
{
    public ARTapToStartSpawner spawner;

    public void OnRestartClicked()
    {
        if (ARGameManager.I != null)
            ARGameManager.I.RestartRound();

        if (spawner != null)
            spawner.ResetSpawnState();
    }
}
