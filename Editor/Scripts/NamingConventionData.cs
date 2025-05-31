using System;
using System.Collections.Generic;

namespace Razluta.UnityAssetNamingConventionEnforcerTool.Editor
{
    [Serializable]
    public enum GameCategory
    {
        CHAR,   // Characters
        WEAP,   // Weapons
        ENV,    // Environment assets
        PROP,   // Props
        GUI,    // UI
        AUDIO,  // Sound effects and music
        VFX     // Visual effects
    }

    [Serializable]
    public enum AssetType
    {
        Prefab,
        Model,
        SkinnedModel,
        Animation,
        Animator,
        Texture,
        Font,
        Sound,
        Music,
        Curve,
        Data,
        Preset,
        Template,
        Atlas,
        Light,
        AI,
        Level,
        Loc // Localization file
    }

    [Serializable]
    public class AssetNamingData
    {
        public string originalName;
        public string originalPath;
        public GameCategory gameCategory;
        public AssetType assetType;
        public string assetName;
        public string variant;
        public string state;
        public string sizeQuality;
        public string finalName;
        
        public AssetNamingData(string originalName, string originalPath)
        {
            this.originalName = originalName;
            this.originalPath = originalPath;
        }
    }
}