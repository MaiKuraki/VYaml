#nullable enable
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace VYaml.Internal
{
    class ExpandBuffer<T>
    {
        const int MinimumGrow = 4;
        const int GrowFactor = 200;
        const int MaxCapacity = 64 * 1024 * 1024;

        public int Length { get; private set; }
        T[] buffer;
        bool isPooled;

        public ExpandBuffer(int capacity)
        {
            if (capacity > MaxCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), 
                    $"Requested capacity {capacity} exceeds maximum of {MaxCapacity}.");
            }
            buffer = ArrayPool<T>.Shared.Rent(capacity);
            isPooled = true;
            Length = 0;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref buffer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => buffer.AsSpan(0, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int length)
        {
            if (length > buffer.Length)
            {
                var newCapacity = buffer.Length * 2;
                while (newCapacity < length)
                {
                    newCapacity *= 2;
                }
                SetCapacity(newCapacity);
            }
            return buffer.AsSpan(0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Length = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek() => ref buffer[Length - 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Pop()
        {
            if (Length == 0)
                throw new InvalidOperationException("Cannot pop the empty buffer");
            return ref buffer[--Length];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T value)
        {
            if (Length == 0)
            {
                value = default!;
                return false;
            }
            value = Pop();
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (Length >= buffer.Length)
            {
                Grow();
            }
            buffer[Length++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetCapacity(int newCapacity)
        {
            if (newCapacity > MaxCapacity)
            {
                throw new InvalidOperationException(
                    $"Buffer capacity {newCapacity} exceeds maximum of {MaxCapacity}.");
            }
            if (buffer.Length >= newCapacity) return;

            var newBuffer = ArrayPool<T>.Shared.Rent(newCapacity);
            buffer.AsSpan(0, Length).CopyTo(newBuffer);
            ReturnBuffer();
            buffer = newBuffer;
            isPooled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReturnBuffer()
        {
            if (isPooled && buffer.Length > 0)
            {
                ArrayPool<T>.Shared.Return(buffer, clearArray: !typeof(T).IsValueType);
                isPooled = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Grow()
        {
            var newCapacity = buffer.Length * GrowFactor / 100;
            if (newCapacity < buffer.Length + MinimumGrow)
            {
                newCapacity = buffer.Length + MinimumGrow;
            }
            SetCapacity(newCapacity);
        }
    }
}
