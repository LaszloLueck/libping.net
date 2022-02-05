namespace LibPing.Net;

public class Either<TL, TR>
{
    private readonly TL _left;
    private readonly TR _right;
    private readonly bool _isLeft;

    public Either(TL left)
    {
        _left = left;
        _isLeft = true;
    }

    public Either(TR right)
    {
        _right = right;
        _isLeft = false;
    }

    public void Match(Action<TL> leftAction, Action<TR> rightAction)
    {
        if (leftAction == null)
        {
            throw new ArgumentNullException(nameof(leftAction));
        }

        if (leftAction == null)
        {
            throw new ArgumentNullException(nameof(leftAction));
        }

        if (_isLeft)
        {
            leftAction(_left);
        }
        else
        {
            rightAction(_right);
        }
    }

    public T Match<T>(Func<TL, T> leftFunc, Func<TR, T> rightFunc)
    {
        if (leftFunc == null)
        {
            throw new ArgumentNullException(nameof(leftFunc));
        }

        if (rightFunc == null)
        {
            throw new ArgumentNullException(nameof(rightFunc));
        }

        return _isLeft ? leftFunc(_left) : rightFunc(_right);
    }

    /// <summary>
    /// If right value is assigned, execute an action on it.
    /// </summary>
    /// <param name="rightAction">Action to execute.</param>
    public void DoRight(Action<TR> rightAction)
    {
        if (rightAction == null)
        {
            throw new ArgumentNullException(nameof(rightAction));
        }

        if (!_isLeft)
        {                
            rightAction(_right);
        }
    }

    public TL LeftOrDefault() => Match(l => l, r => default(TL));

    public TR RightOrDefault() => Match(l => default(TR), r => r);

    public static implicit operator Either<TL, TR>(TL left) => new(left);

    public static implicit operator Either<TL, TR>(TR right) => new(right);
}