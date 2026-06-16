// AnimationHelper.cs — Static coroutine-based tweening utilities
using System;
using System.Collections;
using UnityEngine;

namespace Bloomline.Utilities
{
    /// <summary>
    /// Static helper class providing coroutine-based tweening and easing functions.
    /// All Lerp methods are designed to be started via MonoBehaviour.StartCoroutine.
    /// </summary>
    public static class AnimationHelper
    {
        // ─────────────────────────────────────────────
        //  Easing Functions
        // ─────────────────────────────────────────────

        /// <summary>
        /// Smooth ease-in-out curve (hermite interpolation).
        /// </summary>
        public static float EaseInOut(float t)
        {
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Ease-out curve (decelerating).
        /// </summary>
        public static float EaseOut(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        /// <summary>
        /// Ease-in curve (accelerating).
        /// </summary>
        public static float EaseIn(float t)
        {
            return t * t;
        }

        /// <summary>
        /// Elastic ease-out — overshoots then settles.
        /// </summary>
        public static float EaseOutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            float p = 0.3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p) + 1f;
        }

        /// <summary>
        /// Bounce ease-out — simulates a bouncing effect.
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            if (t < 1f / 2.75f)
                return 7.5625f * t * t;
            if (t < 2f / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }

        /// <summary>
        /// Back ease-out — overshoots slightly then settles. Good for snappy rotations.
        /// </summary>
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        // ─────────────────────────────────────────────
        //  Lerp Coroutines
        // ─────────────────────────────────────────────

        /// <summary>
        /// Lerps a float value from start to end over the given duration,
        /// calling the setter each frame.
        /// </summary>
        /// <param name="from">Start value.</param>
        /// <param name="to">End value.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="setter">Callback invoked each frame with the interpolated value.</param>
        /// <param name="easingFunc">Optional easing function. Defaults to linear if null.</param>
        public static IEnumerator LerpFloat(float from, float to, float duration, Action<float> setter, Func<float, float> easingFunc = null)
        {
            if (duration <= 0f)
            {
                setter(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = easingFunc != null ? easingFunc(t) : t;
                setter(Mathf.LerpUnclamped(from, to, eased));
                yield return null;
            }
            setter(to);
        }

        /// <summary>
        /// Lerps a Vector3 value from start to end over the given duration.
        /// </summary>
        /// <param name="from">Start position/scale.</param>
        /// <param name="to">End position/scale.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="setter">Callback invoked each frame with the interpolated Vector3.</param>
        /// <param name="easingFunc">Optional easing function.</param>
        public static IEnumerator LerpVector3(Vector3 from, Vector3 to, float duration, Action<Vector3> setter, Func<float, float> easingFunc = null)
        {
            if (duration <= 0f)
            {
                setter(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = easingFunc != null ? easingFunc(t) : t;
                setter(Vector3.LerpUnclamped(from, to, eased));
                yield return null;
            }
            setter(to);
        }

        /// <summary>
        /// Lerps a Color value from start to end over the given duration.
        /// </summary>
        /// <param name="from">Start color.</param>
        /// <param name="to">End color.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="setter">Callback invoked each frame with the interpolated Color.</param>
        /// <param name="easingFunc">Optional easing function.</param>
        public static IEnumerator LerpColor(Color from, Color to, float duration, Action<Color> setter, Func<float, float> easingFunc = null)
        {
            if (duration <= 0f)
            {
                setter(to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = easingFunc != null ? easingFunc(t) : t;
                setter(Color.LerpUnclamped(from, to, eased));
                yield return null;
            }
            setter(to);
        }

        // ─────────────────────────────────────────────
        //  Compound Animations
        // ─────────────────────────────────────────────

        /// <summary>
        /// Punch scale animation: scales up then back to original.
        /// Useful for bloom effects on flowers.
        /// </summary>
        /// <param name="transform">The transform to animate.</param>
        /// <param name="punchScale">How much larger the object scales (additive).</param>
        /// <param name="duration">Total duration of the punch.</param>
        public static IEnumerator ScalePunch(Transform transform, float punchScale, float duration)
        {
            if (transform == null) yield break;

            Vector3 originalScale = transform.localScale;
            Vector3 punchedScale = originalScale * (1f + punchScale);
            float halfDuration = duration * 0.4f;
            float returnDuration = duration * 0.6f;

            // Scale up
            yield return LerpVector3(originalScale, punchedScale, halfDuration,
                v => { if (transform != null) transform.localScale = v; },
                EaseOut);

            // Scale back down
            yield return LerpVector3(punchedScale, originalScale, returnDuration,
                v => { if (transform != null) transform.localScale = v; },
                EaseOutElastic);
        }

        /// <summary>
        /// Shake animation: rapidly oscillates position around the original.
        /// Useful for locked tile feedback.
        /// </summary>
        /// <param name="transform">The transform to shake.</param>
        /// <param name="magnitude">Maximum displacement in world units.</param>
        /// <param name="duration">Total shake duration.</param>
        public static IEnumerator Shake(Transform transform, float magnitude, float duration)
        {
            if (transform == null) yield break;

            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Decay magnitude over time
                float currentMagnitude = magnitude * (1f - t);

                // Oscillate using sine for smooth shaking
                float offsetX = Mathf.Sin(elapsed * 40f) * currentMagnitude;
                float offsetY = Mathf.Sin(elapsed * 35f) * currentMagnitude * 0.5f;

                if (transform != null)
                {
                    transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
                }

                yield return null;
            }

            // Snap back to original
            if (transform != null)
            {
                transform.localPosition = originalPos;
            }
        }

        /// <summary>
        /// Pulse animation: fades alpha up and down repeatedly.
        /// Useful for hint highlighting.
        /// </summary>
        /// <param name="renderer">The SpriteRenderer to pulse.</param>
        /// <param name="pulseColor">The color to pulse toward.</param>
        /// <param name="duration">Duration of one pulse cycle.</param>
        /// <param name="cycles">Number of pulse cycles.</param>
        public static IEnumerator Pulse(SpriteRenderer renderer, Color pulseColor, float duration, int cycles = 2)
        {
            if (renderer == null) yield break;

            Color originalColor = renderer.color;
            float cycleDuration = duration / cycles;

            for (int i = 0; i < cycles; i++)
            {
                // Fade to pulse color
                yield return LerpColor(originalColor, pulseColor, cycleDuration * 0.5f,
                    c => { if (renderer != null) renderer.color = c; },
                    EaseInOut);

                // Fade back
                yield return LerpColor(pulseColor, originalColor, cycleDuration * 0.5f,
                    c => { if (renderer != null) renderer.color = c; },
                    EaseInOut);
            }

            // Ensure we end on original color
            if (renderer != null)
            {
                renderer.color = originalColor;
            }
        }
    }
}
