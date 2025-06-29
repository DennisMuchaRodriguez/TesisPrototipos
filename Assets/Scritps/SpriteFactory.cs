using UnityEngine;
using System.Collections.Generic;

public class SpriteFactory
{
    private static SpriteFactory _instance;
    public static SpriteFactory Instance => _instance ??= new SpriteFactory();

    private Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

    public Sprite GetSprite(string spriteName)
    {
        if (!_sprites.ContainsKey(spriteName))
        {
            _sprites[spriteName] = Resources.Load<Sprite>($"Sprites/{spriteName}");
            if (_sprites[spriteName] == null)
            {
                Debug.LogError($"Sprite no encontrado: {spriteName}");
            }
        }
        return _sprites[spriteName];
    }
}
