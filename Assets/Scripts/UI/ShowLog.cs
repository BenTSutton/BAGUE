using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ShowLog : MonoBehaviour
{
    public GameObject logPanelObj;
    public TMP_Text logText1;
    public TMP_Text logText2;
    public TMP_Text logText3;
    public float timeToShowTheLog = 5.0f;

    private static readonly List<string> savedLogs = new();

    private Coroutine hideCoroutine;

    private void Start()
    {
        RefreshLogTexts();
    }

    private void AddLogEntry(string log)
    {
        savedLogs.Add(log);

        if (savedLogs.Count > 3)
            savedLogs.RemoveAt(0);

        RefreshLogTexts();

        ShowTheLogWithSetTime(true);
    }

    private void RefreshLogTexts()
    {
        logText1.text = savedLogs.Count >= 1 ? savedLogs[^1] : "";
        logText2.text = savedLogs.Count >= 2 ? savedLogs[^2] : "";
        logText3.text = savedLogs.Count >= 3 ? savedLogs[^3] : "";
    }

    public void ShowTheLog(bool active, float time)
    {
        logPanelObj.SetActive(active);

        if (active)
        {
            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);

            hideCoroutine = StartCoroutine(AutoHide(time));
        }
        else
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
        }
    }

    public void ShowTheLogWithSetTime(bool active)
    {
        ShowTheLog(active, timeToShowTheLog);
    }

    public void ShowTheLogWithInfiniteTime(bool active)
    {
        ShowTheLog(active, 100000.0f);
    }

    private IEnumerator AutoHide(float delay)
    {
        yield return new WaitForSeconds(delay);

        logPanelObj.SetActive(false);
        hideCoroutine = null;
    }

    public void ConstructLogEntryForResource(string resource, int amount)
    {
        string action = amount >= 0 ? "Gained: " : "Lost: ";
        string log = action + Mathf.Abs(amount) + " " + resource;

        AddLogEntry(log);
    }

    public void ConstructLogEntryForCrew(string crew, bool gained)
    {
        string action = gained ? "Gained: " : "Lost: ";
        string log = action + crew;

        AddLogEntry(log);
    }
}