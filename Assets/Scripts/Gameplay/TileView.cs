// TileView.cs — Visual representation of a tile with animations
using System.Collections;
using UnityEngine;
using Bloomline.Core;
using Bloomline.Utilities;
using Bloomline.Services;
using Bloomline.Data;

namespace Bloomline.Gameplay
{
    /// <summary>
    /// MonoBehaviour visual component for a single tile.
    /// Handles rendering, rotation animation, bloom effects, and state visuals.
    /// Creates placeholder art programmatically.
    /// </summary>
    public class TileView : MonoBehaviour
    {
        private TileModel _model;
        private SpriteRenderer _baseRenderer;
        private SpriteRenderer _channelRenderer;
        private SpriteRenderer _iconRenderer;
        private SpriteRenderer _glowRenderer;
        private SpriteRenderer _lockRenderer;
        private bool _isPowered;
        private bool _isAnimating;
        private Coroutine _currentAnimation;
        private ParticleSystem _particleSystem;

        // Colors for tile types
        private static readonly Color COLOR_TILE_BASE = new Color(0.85f, 0.82f, 0.75f, 1f);      // Stone
        private static readonly Color COLOR_SOURCE = new Color(0.2f, 0.9f, 0.95f, 1f);            // Cyan crystal
        private static readonly Color COLOR_FLOWER_SLEEP = new Color(0.55f, 0.45f, 0.65f, 1f);     // Muted purple
        private static readonly Color COLOR_FLOWER_BLOOM = new Color(1f, 0.55f, 0.75f, 1f);        // Pink bloom
        private static readonly Color COLOR_BLOCKER = new Color(0.35f, 0.32f, 0.3f, 1f);           // Dark stone
        private static readonly Color COLOR_LOCKED_OVERLAY = new Color(0.7f, 0.65f, 0.5f, 0.5f);   // Gold tint
        private static readonly Color COLOR_CHANNEL_UNPOWERED = new Color(0.5f, 0.48f, 0.45f, 1f); // Gray channel
        private static readonly Color COLOR_CHANNEL_POWERED = new Color(0.3f, 1f, 0.9f, 0.9f);     // Glowing cyan
        private static readonly Color COLOR_GLOW = new Color(0.3f, 1f, 0.95f, 0.4f);               // Light glow

        // Colored light variants
        private static readonly Color COLOR_RED_LIGHT = new Color(1f, 0.3f, 0.2f, 0.9f);
        private static readonly Color COLOR_BLUE_LIGHT = new Color(0.2f, 0.5f, 1f, 0.9f);
        private static readonly Color COLOR_YELLOW_LIGHT = new Color(1f, 0.9f, 0.2f, 0.9f);

        /// <summary>The tile model this view represents.</summary>
        public TileModel Model => _model;
        /// <summary>Whether a rotation animation is playing.</summary>
        public bool IsAnimating => _isAnimating;

        /// <summary>
        /// Initialize this tile view with a model and create visuals.
        /// </summary>
        public void Initialize(TileModel model)
        {
            _model = model;
            CreateVisuals();
            UpdateVisuals();
        }

        private void CreateVisuals()
        {
            // Create base tile sprite
            _baseRenderer = CreateSpriteChild("Base", 0);
            _baseRenderer.sprite = CreateSquareSprite();
            _baseRenderer.transform.localScale = Vector3.one * GameConstants.TILE_SIZE * 0.95f;

            // Create channel/path indicator
            if (TileTypeHelper.IsPathTile(_model.Type))
            {
                _channelRenderer = CreateSpriteChild("Channel", 1);
                _channelRenderer.sprite = CreateSquareSprite();
                UpdateChannelShape();
            }

            // Create icon for special tiles (source/flower)
            if (_model.Type == TileType.Source || _model.Type == TileType.Flower)
            {
                _iconRenderer = CreateSpriteChild("Icon", 2);
                _iconRenderer.sprite = CreateIconSprite(_model.Color);
                _iconRenderer.transform.localScale = Vector3.one * GameConstants.TILE_SIZE * 0.35f;
            }

            // Create glow overlay
            _glowRenderer = CreateSpriteChild("Glow", -1);
            _glowRenderer.sprite = CreateCircleSprite();
            _glowRenderer.transform.localScale = Vector3.one * GameConstants.TILE_SIZE * 1.3f;
            _glowRenderer.color = new Color(0, 0, 0, 0); // Start invisible

            // Create lock indicator
            if (_model.IsLocked && (_model.Type == TileType.LockedStraight || _model.Type == TileType.LockedCorner))
            {
                _lockRenderer = CreateSpriteChild("Lock", 3);
                _lockRenderer.sprite = CreateSquareSprite();
                _lockRenderer.transform.localScale = Vector3.one * GameConstants.TILE_SIZE * 0.2f;
                _lockRenderer.transform.localPosition = new Vector3(0.3f, 0.3f, 0);
                _lockRenderer.color = COLOR_LOCKED_OVERLAY;
            }

            // Create particle system
            GameObject psObj = new GameObject("Particles");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;
            _particleSystem = psObj.AddComponent<ParticleSystem>();

            var main = _particleSystem.main;
            main.duration = 1f;
            main.loop = false;
            main.startSpeed = 2f;
            main.startSize = 0.2f;
            main.startLifetime = 0.5f;
            main.playOnAwake = false;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = _particleSystem.emission;
            emission.rateOverTime = 0;

            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = GameConstants.TILE_SIZE * 0.4f;

            var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 5;
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            // Add collider for tap detection
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * GameConstants.TILE_SIZE * 0.95f;
        }

        private SpriteRenderer CreateSpriteChild(string name, int sortingOrder)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        /// <summary>
        /// Update all visuals to match the current model state.
        /// </summary>
        public void UpdateVisuals()
        {
            if (_model == null) return;

            // Update base color
            switch (_model.Type)
            {
                case TileType.Source:
                    _baseRenderer.color = COLOR_SOURCE;
                    if (_iconRenderer != null) _iconRenderer.color = Color.white;
                    break;
                case TileType.Flower:
                    _baseRenderer.color = _isPowered ? COLOR_FLOWER_BLOOM : COLOR_FLOWER_SLEEP;
                    if (_iconRenderer != null)
                    {
                        Color flowerColor = GetColorForTileColor(_model.Color);
                        _iconRenderer.color = _isPowered ? flowerColor : new Color(flowerColor.r * 0.5f, flowerColor.g * 0.5f, flowerColor.b * 0.5f, 1f);
                    }
                    break;
                case TileType.Blocker:
                    _baseRenderer.color = COLOR_BLOCKER;
                    break;
                case TileType.LockedStraight:
                case TileType.LockedCorner:
                    _baseRenderer.color = new Color(COLOR_TILE_BASE.r * 0.85f, COLOR_TILE_BASE.g * 0.85f, COLOR_TILE_BASE.b * 0.8f, 1f);
                    break;
                default:
                    _baseRenderer.color = COLOR_TILE_BASE;
                    break;
            }

            // Update rotation (instant, no animation)
            transform.rotation = Quaternion.Euler(0, 0, -_model.RotationDegrees);

            // Update channel color
            if (_channelRenderer != null)
            {
                _channelRenderer.color = _isPowered ? GetPoweredColor() : COLOR_CHANNEL_UNPOWERED;
            }
        }

        /// <summary>
        /// Update the powered state and refresh visuals.
        /// </summary>
        public void UpdatePoweredState(bool powered)
        {
            bool wasChanged = _isPowered != powered;
            _isPowered = powered;

            if (wasChanged && powered && _particleSystem != null)
            {
                var main = _particleSystem.main;
                main.startColor = GetPoweredColor();
                _particleSystem.Emit(10);
            }

            if (_channelRenderer != null)
            {
                _channelRenderer.color = powered ? GetPoweredColor() : COLOR_CHANNEL_UNPOWERED;
            }

            // Update glow
            if (_glowRenderer != null)
            {
                Color glowColor = powered ? GetGlowColor() : new Color(0, 0, 0, 0);
                _glowRenderer.color = glowColor;
            }

            // Update flower visual
            if (_model.Type == TileType.Flower)
            {
                _baseRenderer.color = powered ? COLOR_FLOWER_BLOOM : COLOR_FLOWER_SLEEP;
                if (_iconRenderer != null)
                {
                    Color flowerColor = GetColorForTileColor(_model.Color);
                    _iconRenderer.color = powered ? flowerColor : new Color(flowerColor.r * 0.5f, flowerColor.g * 0.5f, flowerColor.b * 0.5f, 1f);
                }
            }
        }

        /// <summary>
        /// Animate smooth 90° clockwise rotation.
        /// </summary>
        public void AnimateRotation()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }
            _currentAnimation = StartCoroutine(RotationCoroutine());
        }

        /// <summary>
        /// Animate flower blooming effect.
        /// </summary>
        public void AnimateBloom()
        {
            if (_model.Type != TileType.Flower) return;
            if (_particleSystem != null)
            {
                var main = _particleSystem.main;
                main.startColor = GetColorForTileColor(_model.Color);
                _particleSystem.Emit(20);
            }
            StartCoroutine(BloomCoroutine());
        }

        /// <summary>
        /// Animate locked tile shake feedback.
        /// </summary>
        public void AnimateShake()
        {
            bool reducedMotion = false;
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                reducedMotion = saveService.LoadSettings().reducedMotionEnabled;
            }

            if (reducedMotion) return;

            StartCoroutine(ShakeCoroutine());
        }

        /// <summary>
        /// Animate hint pulse.
        /// </summary>
        public void AnimateHintPulse()
        {
            StartCoroutine(HintPulseCoroutine());
        }

        private IEnumerator RotationCoroutine()
        {
            _isAnimating = true;
            float targetAngle = -_model.RotationDegrees;
            float startAngle = targetAngle + 90f; // Started from 90° before
            float elapsed = 0f;

            bool reducedMotion = false;
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                reducedMotion = saveService.LoadSettings().reducedMotionEnabled;
            }

            if (reducedMotion)
            {
                transform.rotation = Quaternion.Euler(0, 0, targetAngle);
                _isAnimating = false;
                _currentAnimation = null;
                UpdateChannelShape();
                yield break;
            }

            while (elapsed < GameConstants.ROTATION_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / GameConstants.ROTATION_DURATION);
                t = AnimationHelper.EaseOutBack(t);
                float angle = Mathf.LerpAngle(startAngle, targetAngle, t);
                transform.rotation = Quaternion.Euler(0, 0, angle);
                yield return null;
            }

            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
            _isAnimating = false;
            _currentAnimation = null;

            UpdateChannelShape();
        }

        private IEnumerator BloomCoroutine()
        {
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.15f;

            // Scale up
            while (elapsed < GameConstants.BLOOM_DURATION * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / (GameConstants.BLOOM_DURATION * 0.5f));
                t = AnimationHelper.EaseOutBack(t);
                transform.localScale = Vector3.LerpUnclamped(originalScale, targetScale, t);
                yield return null;
            }

            // Scale back
            elapsed = 0f;
            while (elapsed < GameConstants.BLOOM_DURATION * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / (GameConstants.BLOOM_DURATION * 0.5f));
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private IEnumerator ShakeCoroutine()
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < GameConstants.SHAKE_DURATION)
            {
                elapsed += Time.deltaTime;
                float strength = (1f - elapsed / GameConstants.SHAKE_DURATION) * GameConstants.SHAKE_MAGNITUDE;
                float offsetX = Mathf.Sin(elapsed * 40f) * strength;
                transform.localPosition = originalPos + new Vector3(offsetX, 0, 0);
                yield return null;
            }

            transform.localPosition = originalPos;
        }

        private IEnumerator HintPulseCoroutine()
        {
            float elapsed = 0f;
            Color originalColor = _baseRenderer.color;
            Color pulseColor = new Color(1f, 0.9f, 0.3f, 1f); // Golden hint

            // Pulse 3 times
            for (int i = 0; i < 3; i++)
            {
                elapsed = 0f;
                while (elapsed < GameConstants.HINT_PULSE_DURATION / 3f)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.PingPong(elapsed * 6f, 1f);
                    _baseRenderer.color = Color.Lerp(originalColor, pulseColor, t);
                    yield return null;
                }
            }

            _baseRenderer.color = originalColor;
        }

        private void UpdateChannelShape()
        {
            if (_channelRenderer == null) return;

            Direction openings = _model.CurrentOpenings;

            // Simple visual: scale based on type
            bool isHorizontal = (openings & Direction.East) != 0 && (openings & Direction.West) != 0;
            bool isVertical = (openings & Direction.North) != 0 && (openings & Direction.South) != 0;

            if (_model.Type == TileType.Straight || _model.Type == TileType.LockedStraight)
            {
                // Straight: thin bar
                _channelRenderer.transform.localScale = new Vector3(
                    GameConstants.TILE_SIZE * 0.25f,
                    GameConstants.TILE_SIZE * 0.85f,
                    1f);
                _channelRenderer.transform.localRotation = Quaternion.identity;
            }
            else if (_model.Type == TileType.Corner || _model.Type == TileType.LockedCorner)
            {
                // Corner: L-shape approximated as a square in corner
                _channelRenderer.transform.localScale = new Vector3(
                    GameConstants.TILE_SIZE * 0.25f,
                    GameConstants.TILE_SIZE * 0.55f,
                    1f);
                _channelRenderer.transform.localPosition = new Vector3(0.1f, 0.1f, 0);
                _channelRenderer.transform.localRotation = Quaternion.Euler(0, 0, 45f);
            }
            else if (_model.Type == TileType.Source)
            {
                _channelRenderer.transform.localScale = new Vector3(
                    GameConstants.TILE_SIZE * 0.6f,
                    GameConstants.TILE_SIZE * 0.25f,
                    1f);
                _channelRenderer.transform.localRotation = Quaternion.identity;
            }
            else if (_model.Type == TileType.Flower)
            {
                _channelRenderer.transform.localScale = new Vector3(
                    GameConstants.TILE_SIZE * 0.6f,
                    GameConstants.TILE_SIZE * 0.25f,
                    1f);
                _channelRenderer.transform.localRotation = Quaternion.identity;
            }
        }

        private Color GetPoweredColor()
        {
            switch (_model.Color)
            {
                case TileColor.Red: return COLOR_RED_LIGHT;
                case TileColor.Blue: return COLOR_BLUE_LIGHT;
                case TileColor.Yellow: return COLOR_YELLOW_LIGHT;
                default: return COLOR_CHANNEL_POWERED;
            }
        }

        private Color GetGlowColor()
        {
            Color powered = GetPoweredColor();
            return new Color(powered.r, powered.g, powered.b, 0.3f);
        }

        private Color GetColorForTileColor(TileColor tileColor)
        {
            switch (tileColor)
            {
                case TileColor.Red: return new Color(1f, 0.3f, 0.3f, 1f);
                case TileColor.Blue: return new Color(0.3f, 0.5f, 1f, 1f);
                case TileColor.Yellow: return new Color(1f, 0.85f, 0.2f, 1f);
                default: return Color.white;
            }
        }

        /// <summary>
        /// Create a simple white square sprite at runtime.
        /// </summary>
        public static Sprite CreateSquareSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                // Rounded corners
                float dx = Mathf.Abs(x - 31.5f);
                float dy = Mathf.Abs(y - 31.5f);
                float cornerRadius = 8f;
                
                float distToCorner = Mathf.Sqrt((dx - 24f) * (dx - 24f) + (dy - 24f) * (dy - 24f));
                bool isOutside = dx > 24f && dy > 24f && distToCorner >= cornerRadius;
                
                if (isOutside)
                {
                    pixels[i] = Color.clear;
                }
                else
                {
                    // Gradient based on y (subtle)
                    float gradient = Mathf.Lerp(0.85f, 1.0f, y / 63f);
                    
                    // Inner border
                    bool isBorder = (dx > 29f || dy > 29f) || 
                                    (dx > 24f && dy > 24f && distToCorner > cornerRadius - 2f);
                                    
                    if (isBorder)
                    {
                        pixels[i] = new Color(0.7f * gradient, 0.7f * gradient, 0.7f * gradient, 1f);
                    }
                    else
                    {
                        pixels[i] = new Color(gradient, gradient, gradient, 1f);
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        private static Sprite CreateIconSprite(TileColor tileColor)
        {
            bool cbMode = false;
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                cbMode = saveService.LoadSettings().colorblindModeEnabled;
            }

            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                float dx = x - 31.5f;
                float dy = y - 31.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float edge = 28f;

                if (dist < edge)
                {
                    bool isPattern = false;
                    if (cbMode)
                    {
                        if (tileColor == TileColor.Red)
                        {
                            if ((x % 16 < 8) && (y % 16 < 8)) isPattern = true; // Dots
                        }
                        else if (tileColor == TileColor.Blue)
                        {
                            if ((x + y) % 16 < 8) isPattern = true; // Stripes
                        }
                        else if (tileColor == TileColor.Yellow)
                        {
                            if (Mathf.Abs(dx) < 6 || Mathf.Abs(dy) < 6) isPattern = true; // Cross
                        }
                    }

                    pixels[i] = isPattern ? new Color(0.3f, 0.3f, 0.3f, 1f) : Color.white;
                }
                else if (dist < edge + 2f)
                {
                    pixels[i] = new Color(1, 1, 1, 1f - (dist - edge) / 2f);
                }
                else
                {
                    pixels[i] = Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        /// <summary>
        /// Create a simple white circle sprite at runtime.
        /// </summary>
        public static Sprite CreateCircleSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                float dx = x - 31.5f;
                float dy = y - 31.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float edge = 28f;
                if (dist < edge)
                    pixels[i] = Color.white;
                else if (dist < edge + 2f)
                    pixels[i] = new Color(1, 1, 1, 1f - (dist - edge) / 2f);
                else
                    pixels[i] = Color.clear;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }
    }
}
