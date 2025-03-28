using System;
using System.Collections.Generic;
using UnityEngine;

public class FunctionTimer : MonoBehaviour
{
    private static List<FunctionTimer> timerList = new List<FunctionTimer>();

    public static FunctionTimer Create(Action action, float timer, string functionName = "", bool useUnscaledDeltaTime = false, bool stopAllWithSameName = false)
    {
        // Vérifie si un timer avec le même nom doit être stoppé
        if (stopAllWithSameName)
        {
            StopAllTimersWithName(functionName);
        }

        // Création de l'objet et ajout du composant
        GameObject obj = new GameObject("FunctionTimer Object " + functionName);
        FunctionTimer funcTimer = obj.AddComponent<FunctionTimer>();
        funcTimer.Setup(action, timer, functionName, useUnscaledDeltaTime);

        timerList.Add(funcTimer);

        return funcTimer;
    }

    private Action action;
    private float timer;
    private string functionName;
    private bool useUnscaledDeltaTime;

    public void Setup(Action action, float timer, string functionName, bool useUnscaledDeltaTime)
    {
        this.action = action;
        this.timer = timer;
        this.functionName = functionName;
        this.useUnscaledDeltaTime = useUnscaledDeltaTime;
    }

    private void Update()
    {
        timer -= useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (timer <= 0)
        {
            action?.Invoke();
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        timerList.Remove(this);
        Destroy(gameObject);
    }

    public static void StopAllTimersWithName(string functionName)
    {
        for (int i = timerList.Count - 1; i >= 0; i--)
        {
            if (timerList[i].functionName == functionName)
            {
                timerList[i].DestroySelf();
            }
        }
    }
}
