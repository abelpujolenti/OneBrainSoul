using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Player;
using Player.Movement;
using System.Globalization;
using Managers;

public class PlayerMetrics : MonoBehaviour
{
    [SerializeField] float frequency = 1f;

    PlayerCharacterController player;
    TextAsset file;
    float logTime = 0f;
    string dirPath;
    string filePath;
    private static IMovementHandler prevMovementHandler;

    private void Start()
    {
#if UNITY_EDITOR
        return;
#endif
        player = GetComponent<PlayerCharacterController>();
        CreateFile();

        EventsManager.OnDefeatEnemy += DefeatAgent;
    }

    private void Update()
    {
#if UNITY_EDITOR
        return;
#endif
        logTime += Time.deltaTime;
        if (logTime < frequency) return;

        logTime -= frequency;
        File.AppendAllTextAsync(filePath, "\n" + (Time.timeSinceLevelLoadAsDouble * 1000d).ToString(CultureInfo.InvariantCulture) + " " + 
            player.transform.position.ToString() + " " + 
            player.GetCamera().transform.rotation.eulerAngles.ToString() + " " +
            player.GetMovementHandler().ToString() + " " +
            (player.IsInCombat() ? "InCombat" : "OutOfCombat") + " " +
            (prevMovementHandler == player.GetMovementHandler() ? "" : player.GetMovementHandler() is GroundedMovementHandler ? "Grounded" : player.GetMovementHandler() is AirborneMovementHandler && prevMovementHandler is GroundedMovementHandler ? "Jumped" : "UsedAbility"));

        prevMovementHandler = player.GetMovementHandler();
    }

    public void DefeatAgent()
    {
#if UNITY_EDITOR
        return;
#endif
        File.AppendAllTextAsync(filePath, "\n" + (Time.timeSinceLevelLoadAsDouble * 1000d).ToString(CultureInfo.InvariantCulture) + " " +
        player.transform.position.ToString() + " " +
        player.GetCamera().transform.rotation.eulerAngles.ToString() + " " +
        player.GetMovementHandler().ToString() + " " +
        (player.IsInCombat() ? "InCombat" : "OutOfCombat") + " " +
        "Killed");
    }

    private void CreateFile()
    {
        dirPath = Application.streamingAssetsPath + "/Metrics/";
        filePath = string.Format(dirPath + "{0}.txt",
                     System.DateTime.Now.ToString("yy-MM-dd_HH.mm.ss")
                     );
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        File.WriteAllTextAsync(filePath, "#PLAYER METRICS " + System.DateTime.Now.ToString("yy-MM-dd_HH.mm.ss"));
    }
}
