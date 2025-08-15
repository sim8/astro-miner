// TransitionManager.cs

using System;
using Microsoft.Xna.Framework;

namespace AstroMiner;

public class TransitionManager(BaseGame game)
{
    private Action _onFadeCompleteCallback;
    private float _targetOpacity;
    private float _transitionSpeed = 1f; // seconds for full transition
    public float Opacity { get; private set; }

    public bool IsTransitioning => Opacity != _targetOpacity;

    public void Update(GameTime gameTime)
    {
        if (IsTransitioning)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds / _transitionSpeed;

            if (Opacity < _targetOpacity)
                Opacity = MathHelper.Min(Opacity + delta, _targetOpacity);
            else
                Opacity = MathHelper.Max(Opacity - delta, _targetOpacity);

            if (Opacity == _targetOpacity && _onFadeCompleteCallback != null)
            {
                var callback = _onFadeCompleteCallback;
                _onFadeCompleteCallback = null;
                callback();
            }
        }
    }

    private void FadeTo(float target, float speed = 1f, Action onComplete = null)
    {
        _targetOpacity = MathHelper.Clamp(target, 0f, 1f);
        _transitionSpeed = speed;
        _onFadeCompleteCallback = onComplete;
    }

    public void FadeIn(float speed = 1f, Action onComplete = null)
    {
        FadeTo(0f, speed, onComplete);
    }

    public void FadeOut(float speed = 1f, Action onComplete = null)
    {
        FadeTo(1f, speed, onComplete);
    }
}