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
Step 4.2: Naming Convention Logic
Create Editor/Scripts/NamingConventionProcessor.cs:
csharpusing System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Razluta.UnityAssetNamingConventionEnforcerTool.Editor
{
    public static class NamingConventionProcessor
    {
        private static readonly Dictionary<string, AssetType> ExtensionToAssetType = new Dictionary<string, AssetType>
        {
            { ".prefab", AssetType.Prefab },
            { ".fbx", AssetType.Model },
            { ".obj", AssetType.Model },
            { ".dae", AssetType.Model },
            { ".3ds", AssetType.Model },
            { ".blend", AssetType.Model },
            { ".max", AssetType.Model },
            { ".ma", AssetType.Model },
            { ".mb", AssetType.Model },
            { ".anim", AssetType.Animation },
            { ".controller", AssetType.Animator },
            { ".png", AssetType.Texture },
            { ".jpg", AssetType.Texture },
            { ".jpeg", AssetType.Texture },
            { ".tga", AssetType.Texture },
            { ".psd", AssetType.Texture },
            { ".tiff", AssetType.Texture },
            { ".gif", AssetType.Texture },
            { ".bmp", AssetType.Texture },
            { ".ttf", AssetType.Font },
            { ".otf", AssetType.Font },
            { ".wav", AssetType.Sound },
            { ".mp3", AssetType.Sound },
            { ".ogg", AssetType.Sound },
            { ".aiff", AssetType.Sound },
            { ".asset", AssetType.Data },
            { ".json", AssetType.Data },
            { ".xml", AssetType.Data },
            { ".txt", AssetType.Data }
        };

        private static readonly HashSet<string> StateKeywords = new HashSet<string>
        {
            "idle", "walk", "run", "jump", "attack", "dead", "damaged", "normal", "active", "inactive",
            "open", "closed", "on", "off", "empty", "full", "broken", "whole"
        };

        private static readonly HashSet<string> SizeQualityKeywords = new HashSet<string>
        {
            "1k", "2k", "4k", "8k", "low", "medium", "high", "ultra", "small", "large", "xl", "xs"
        };

        private static readonly HashSet<string> VariantKeywords = new HashSet<string>
        {
            "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "a", "b", "c", "d", "e", "alt", "var", "variant"
        };

        public static AssetType InferAssetTypeFromPath(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLower();
            return ExtensionToAssetType.TryGetValue(extension, out AssetType assetType) ? assetType : AssetType.Data;
        }

        public static AssetNamingData ProcessAssetName(string originalName, string assetPath, GameCategory gameCategory, AssetType assetType)
        {
            var data = new AssetNamingData(originalName, assetPath)
            {
                gameCategory = gameCategory,
                assetType = assetType
            };

            // Clean the original name and split into parts
            string cleanName = CleanOriginalName(originalName, gameCategory, assetType);
            var nameParts = SplitIntoWords(cleanName);

            // Categorize parts
            CategorizeNameParts(nameParts, data);

            // Build final name
            data.finalName = BuildFinalName(data);

            return data;
        }

        private static string CleanOriginalName(string originalName, GameCategory gameCategory, AssetType assetType)
        {
            string cleanName = originalName;
            
            // Remove file extension if present
            cleanName = Path.GetFileNameWithoutExtension(cleanName);
            
            // Remove existing category and asset type prefixes if they exist
            var categoryNames = Enum.GetNames(typeof(GameCategory));
            var assetTypeNames = Enum.GetNames(typeof(AssetType));
            
            foreach (var category in categoryNames)
            {
                if (cleanName.StartsWith(category + "_", StringComparison.OrdinalIgnoreCase))
                {
                    cleanName = cleanName.Substring(category.Length + 1);
                    break;
                }
            }
            
            foreach (var type in assetTypeNames)
            {
                if (cleanName.StartsWith(type + "_", StringComparison.OrdinalIgnoreCase))
                {
                    cleanName = cleanName.Substring(type.Length + 1);
                    break;
                }
            }

            return cleanName;
        }

        private static List<string> SplitIntoWords(string name)
        {
            // Split on spaces, underscores, and camelCase
            var words = new List<string>();
            
            // First split on spaces and underscores
            var parts = name.Split(new char[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                // Then split camelCase
                var camelCaseParts = Regex.Split(part, @"(?<!^)(?=[A-Z])");
                words.AddRange(camelCaseParts);
            }
            
            return words.Where(w => !string.IsNullOrWhiteSpace(w)).ToList();
        }

        private static void CategorizeNameParts(List<string> nameParts, AssetNamingData data)
        {
            var remainingParts = new List<string>();
            var variantParts = new List<string>();
            var stateParts = new List<string>();
            var sizeParts = new List<string>();

            foreach (var part in nameParts)
            {
                string lowerPart = part.ToLower();
                
                if (VariantKeywords.Contains(lowerPart) || IsNumericVariant(part))
                {
                    variantParts.Add(part);
                }
                else if (StateKeywords.Contains(lowerPart))
                {
                    stateParts.Add(part);
                }
                else if (SizeQualityKeywords.Contains(lowerPart))
                {
                    sizeParts.Add(part);
                }
                else
                {
                    remainingParts.Add(part);
                }
            }

            data.assetName = string.Join("_", remainingParts);
            data.variant = string.Join("_", variantParts);
            data.state = string.Join("_", stateParts);
            data.sizeQuality = string.Join("_", sizeParts);
        }

        private static bool IsNumericVariant(string part)
        {
            return Regex.IsMatch(part, @"^\d{1,2}$");
        }

        private static string BuildFinalName(AssetNamingData data)
        {
            var parts = new List<string>
            {
                data.gameCategory.ToString(),
                data.assetType.ToString()
            };

            if (!string.IsNullOrEmpty(data.assetName))
                parts.Add(data.assetName);
            
            if (!string.IsNullOrEmpty(data.variant))
                parts.Add(data.variant);
            
            if (!string.IsNullOrEmpty(data.state))
                parts.Add(data.state);
            
            if (!string.IsNullOrEmpty(data.sizeQuality))
                parts.Add(data.sizeQuality);

            return string.Join("_", parts);
        }

        public static string ValidateAssetName(string name)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(name))
            {
                errors.Add("Name cannot be empty");
                return string.Join(", ", errors);
            }

            if (name.Contains(" "))
                errors.Add("Name cannot contain spaces");

            if (name.Contains(".."))
                errors.Add("Name cannot contain consecutive dots");

            var invalidChars = new char[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/' };
            if (name.IndexOfAny(invalidChars) >= 0)
                errors.Add("Name contains invalid characters");

            if (name.Length > 200)
                errors.Add("Name is too long (max 200 characters)");

            return errors.Count > 0 ? string.Join(", ", errors) : null;
        }
    }
}