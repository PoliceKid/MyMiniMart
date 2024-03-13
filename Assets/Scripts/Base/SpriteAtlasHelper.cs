using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
public static class SpriteAtlasHelper
{
    public static Dictionary<string, SpriteAtlas> SpriteAtlas;
    public static void LoadAsset()
    {
        SpriteAtlas = new Dictionary<string, SpriteAtlas>();
        SpriteAtlas atlas = Resources.Load<SpriteAtlas>("");
    }
    public static Sprite GetSpriteFromAtlas(string atlasName, string spriteName)
    {
        if (SpriteAtlas.ContainsKey(atlasName)) return null;
        // Get the sprite from the atlas by name
        Sprite sprite = SpriteAtlas[atlasName].GetSprite(spriteName);

        if (sprite == null)
        {
            Debug.LogError("Sprite not found in atlas: " + spriteName);
        }

        return sprite;
    }
    public static Sprite[] GetAllSpritesFromAtlas(string atlasName)
    {
        // Load the sprite atlas by name
        if (SpriteAtlas.ContainsKey(atlasName)) return null;

        // Get all sprites from the atlas
        Sprite[] sprites = new Sprite[SpriteAtlas[atlasName].spriteCount];
        SpriteAtlas[atlasName].GetSprites(sprites);

        return sprites;
    }
}
