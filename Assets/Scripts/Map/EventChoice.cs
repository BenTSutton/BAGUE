using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventChoice
{
    [Tooltip("Text to be shown describing what the choice is")]
    public string choiceText;
    [Tooltip("Text to be shown describing what the outcome is")]
    public string outcomeText;
    [Tooltip("List of effects that happen when you select this outcome")]
    public List<EventEffectData> effects = new List<EventEffectData>();
}