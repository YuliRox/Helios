using System;
using System.Collections.Generic;

namespace Helios;

public class OperationSession : IDisposable
{
    private static readonly Stack<OperationSession> sessions = new();
    public static OperationSession Current
    {
        get
        {
            return sessions.Count > 0 ? sessions.Peek() : null;
        }
    }

    public OperationSession()
    {
        _parent = Current;
        sessions.Push(this);
    }

    private readonly OperationSession _parent;
    public OperationSession Parent
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(OperationSession));
            return _parent;
        }
    }

    private readonly List<(string Topic, string Message)> _messageBuffer = new();
    public List<(string Topic, string Message)> MessageBuffer
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(OperationSession));
            return _messageBuffer;
        }
    }

    private string _lastSendMessage;
    public string LastSendMessage
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(OperationSession));
            return _lastSendMessage;
        }
        set
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(OperationSession));
            _lastSendMessage = value;
        }
    }

    protected bool isDisposed = false;

    public void Dispose()
    {
        if (isDisposed)
            return;

        isDisposed = true;
        sessions.Pop();
        GC.SuppressFinalize(this);
    }
}