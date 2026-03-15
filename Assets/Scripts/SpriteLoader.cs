using UnityEngine;
using System.Collections.Generic;

public class SpriteLoader : MonoBehaviour
{
    public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    void Awake()
    {
        Sprite[] loadedIcons = Resources.LoadAll<Sprite>("UI");

        foreach (var sprite in loadedIcons)
        {
            sprites[sprite.name] = sprite;
        }
    }
}
