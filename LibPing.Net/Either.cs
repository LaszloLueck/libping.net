namespace LibPing.Net;

/// <summary>
/// Either class for handling right and left cases
/// </summary>
/// <typeparam name="TLeft"></typeparam>
/// <typeparam name="TRight"></typeparam>
public class Either<TLeft, TRight>
{
    private readonly TLeft? _left;
    private readonly TRight? _right;
    private readonly bool _isLeft;

    /// <summary>
    /// Either parameter declaration for Left
    /// </summary>
    /// <param name="left"></param>
    private Either(TLeft left)
    {
        _left = left;
        _isLeft = true;
    }

    /// <summary>
    /// Either parameter declartion for Right
    /// </summary>
    /// <param name="right"></param>
    private Either(TRight right)
    {
        _right = right;
        _isLeft = false;
    }

    /// <summary>
    /// Matches about right or left case and invoke the appropriate action
    /// </summary>
    /// <param name="leftAction"></param>
    /// <param name="rightAction"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Match(Action<TLeft> leftAction, Action<TRight> rightAction)
    {
        if (leftAction == null)
        {
            throw new ArgumentNullException(nameof(leftAction));
        }

        if (rightAction == null)
        {
            throw new ArgumentNullException(nameof(rightAction));
        }

        if (_isLeft)
        {
            if (_left is null) throw new ArgumentNullException(nameof(_left));
            leftAction(_left);
        }
        else
        {
            if (_right is null) throw new ArgumentNullException(nameof(_right));
            rightAction(_right);
        }
    }

    /// <summary>
    /// Matches about right or left case, invoke the appropriate function, and return their result
    /// </summary>
    /// <param name="leftFunc"></param>
    /// <param name="rightFunc"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T Match<T>(Func<TLeft, T> leftFunc, Func<TRight, T> rightFunc)
    {
        if (leftFunc == null)
        {
            throw new ArgumentNullException(nameof(leftFunc));
        }

        if (rightFunc == null)
        {
            throw new ArgumentNullException(nameof(rightFunc));
        }

        return _isLeft switch
        {
            true when _left is null => throw new ArgumentNullException(nameof(_left)),
            false when _right is null => throw new ArgumentNullException(nameof(_right)),
            _ => _isLeft ? leftFunc.Invoke(_left!) : rightFunc.Invoke(_right!)
        };
    }

    /// <summary>
    /// If right value is assigned, execute an action on it.
    /// </summary>
    /// <param name="rightAction">Action to execute.</param>
    public void DoRight(Action<TRight> rightAction)
    {
        if (rightAction == null)
        {
            throw new ArgumentNullException(nameof(rightAction));
        }


        if (_isLeft) return;
        if (_right is null) throw new ArgumentNullException(nameof(_right));
        rightAction(_right);
    }

    /// <summary>
    /// returns left value or, if not the case, the default.
    /// </summary>
    /// <returns></returns>
    public TLeft? LeftOrDefault() => Match(l => l, r => default(TLeft));

    /// <summary>
    /// returns right value, or if not the case, the default
    /// </summary>
    /// <returns></returns>
    public TRight? RightOrDefault() => Match(l => default(TRight), r => r);

    /// <summary>
    /// Either operator for the left case
    /// </summary>
    /// <param name="left"></param>
    /// <returns></returns>
    public static implicit operator Either<TLeft, TRight>(TLeft left) => new(left);

    /// <summary>
    /// Either operator for the right case
    /// </summary>
    /// <param name="right"></param>
    /// <returns></returns>
    public static implicit operator Either<TLeft, TRight>(TRight right) => new(right);
}