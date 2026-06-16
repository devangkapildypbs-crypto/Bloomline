// LevelLoader.cs — Loads level data from Resources
using System;
using UnityEngine;
using Bloomline.Core;
using Bloomline.Gameplay;

namespace Bloomline.Levels
{
    /// <summary>
    /// Static utility class for loading level data from JSON files in the Resources folder.
    /// Levels are stored at Resources/Levels/level_001.json, level_002.json, etc.
    /// </summary>
    public static class LevelLoader
    {
        /// <summary>
        /// Loads a level by its level number from Resources.
        /// Expects a JSON file at Resources/Levels/level_XXX where XXX is zero-padded.
        /// </summary>
        /// <param name="levelNumber">The 1-based level number to load.</param>
        /// <returns>Parsed LevelData, or null if the level file was not found.</returns>
        public static LevelData LoadLevel(int levelNumber)
        {
            string fileName = string.Format("level_{0:D3}", levelNumber);
            string resourcePath = GameConstants.LEVEL_RESOURCE_PATH + fileName;

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogError($"[LevelLoader] Level file not found at Resources/{resourcePath}");
                return null;
            }

            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonAsset.text);
            if (levelData == null)
            {
                Debug.LogError($"[LevelLoader] Failed to parse level JSON: {resourcePath}");
                return null;
            }

            // Ensure level metadata is set
            if (string.IsNullOrEmpty(levelData.levelId))
            {
                levelData.levelId = fileName;
            }
            if (levelData.levelNumber == 0)
            {
                levelData.levelNumber = levelNumber;
            }

            Debug.Log($"[LevelLoader] Loaded level {levelNumber}: {levelData.gridWidth}x{levelData.gridHeight} with {levelData.tiles.Count} tiles");
            return levelData;
        }

        /// <summary>
        /// Loads a level from a raw JSON string. Useful for testing and level editors.
        /// </summary>
        /// <param name="json">JSON string representing a LevelData.</param>
        /// <returns>Parsed LevelData, or null on failure.</returns>
        public static LevelData LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[LevelLoader] Empty JSON string provided");
                return null;
            }

            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            return levelData;
        }

        /// <summary>
        /// Parses a tile type string into the TileType enum.
        /// Case-insensitive matching.
        /// </summary>
        /// <param name="typeString">String representation of the tile type (e.g., "Source", "Straight").</param>
        /// <returns>The matching TileType, or TileType.Empty if not recognized.</returns>
        public static TileType ParseTileType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
                return TileType.Empty;

            // Try standard enum parse first
            if (Enum.TryParse<TileType>(typeString, true, out TileType result))
            {
                return result;
            }

            // Manual fallback for common aliases
            switch (typeString.ToLowerInvariant())
            {
                case "source":         return TileType.Source;
                case "flower":         return TileType.Flower;
                case "straight":       return TileType.Straight;
                case "corner":         return TileType.Corner;
                case "blocker":        return TileType.Blocker;
                case "lockedstraight": return TileType.LockedStraight;
                case "locked_straight": return TileType.LockedStraight;
                case "lockedcorner":   return TileType.LockedCorner;
                case "locked_corner":  return TileType.LockedCorner;
                case "splitter":       return TileType.Splitter;
                case "oneway":         return TileType.OneWay;
                case "one_way":        return TileType.OneWay;
                case "bridge":         return TileType.Bridge;
                case "switch":         return TileType.Switch;
                case "gate":           return TileType.Gate;
                case "empty":
                default:
                    return TileType.Empty;
            }
        }

        /// <summary>
        /// Parses a color string into the TileColor enum.
        /// Case-insensitive matching.
        /// </summary>
        /// <param name="colorString">String representation of the tile color (e.g., "Red", "Blue").</param>
        /// <returns>The matching TileColor, or TileColor.White if not recognized.</returns>
        public static TileColor ParseTileColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString))
                return TileColor.White;

            if (Enum.TryParse<TileColor>(colorString, true, out TileColor result))
            {
                return result;
            }

            switch (colorString.ToLowerInvariant())
            {
                case "red":    return TileColor.Red;
                case "blue":   return TileColor.Blue;
                case "yellow": return TileColor.Yellow;
                case "white":
                default:
                    return TileColor.White;
            }
        }
    }
}
