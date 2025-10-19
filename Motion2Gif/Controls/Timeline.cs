using System;
using Avalonia;

namespace Motion2Gif.Controls;

public record struct Timeline(Rect Box)
{
    private bool _isPressed = false;

    public void Pressed(Point pointPosition, Action action)
    {
        if (!Box.Contains(pointPosition)) 
            return;

        _isPressed = true;
        action();
    }

    public void Moved(Point pointPosition, Action action)
    {
        if (!_isPressed)
            return;

        if (!Box.Contains(pointPosition)) 
            return;

        action();
    }

    public void Unpressed(Point pointPosition)
    {
        if (!_isPressed)
            return;

        if (!Box.Contains(pointPosition)) 
            return;

        _isPressed = false;
    }

    public void Release()
    {
        this._isPressed = false;
    }
}