using UnityEngine;

public class RestartHook : MonoBehaviour
{
    public ARTapToStartSpawner spawner;

    public void OnRestartClicked()
    {
        if (ARGameManager.I != null)
            ARGameManager.I.RestartRound();

       
        if (spawner == null)
            spawner = FindAnyObjectByType<ARTapToStartSpawner>();

        if (spawner != null)
            spawner.ResetSpawnState(true);
        else
            Debug.LogError("ARTapToStartSpawner를 찾을 수 없음 (씬에 스포너가 있는지 확인)");
    }
}
