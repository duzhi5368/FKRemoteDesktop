using System;
using System.Collections.Generic;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    // 多线程操作安全的字节缓冲池，用来缓存接受的消息
    public class BufferPool
    {
        public event EventHandler NewBufferAllocated;   // 通知 listeners 分配了一个超出缓冲池的新缓冲块

        // 缓冲区不足的处理事件
        protected virtual void OnNewBufferAllocated(EventArgs e)
        {
            var handler = NewBufferAllocated;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler BufferRequested;      // 通知 listeners 有新的缓冲块被分配

        // 新缓冲区被分配的事件
        protected virtual void OnBufferRequested(EventArgs e)
        {
            var handler = BufferRequested;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler BufferReturned;       // 通知 listeners 有新的缓冲块被返还

        // 新缓冲区被返还事件
        protected virtual void OnBufferReturned(EventArgs e)
        {
            var handler = BufferReturned;
            if (handler != null)
                handler(this, e);
        }

        private readonly int _bufferLength;
        private int _bufferCount;
        private readonly Stack<byte[]> _buffers;

        public int BufferLength { get { return _bufferLength; } }   // 获取缓冲池中的一个缓冲块大小
        public int MaxBufferCount { get { return _bufferCount; } }  // 获取当前缓冲池中最大缓冲块的个数
        public int BuffersAvailable => _buffers.Count;              // 获取当前缓冲池中可用的缓冲块个数
        public bool ClearOnReturn { get; set; }                     // 当缓冲块 返还到 缓冲池时，是否自动清空

        /// <summary>
        /// 创建一个缓冲池
        /// </summary>
        /// <param name="baseBufferLength">预分配每个缓冲块的大小</param>
        /// <param name="baseBufferCount">预分配每个缓冲池中的缓冲块个数</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public BufferPool(int baseBufferLength, int baseBufferCount)
        {
            if (baseBufferLength <= 0)
                throw new ArgumentOutOfRangeException("baseBufferLength", baseBufferLength, "Buffer length must be a positive integer value.");
            if (baseBufferCount <= 0)
                throw new ArgumentOutOfRangeException("baseBufferCount", baseBufferCount, "Buffer count must be a positive integer value.");

            _bufferLength = baseBufferLength;
            _bufferCount = baseBufferCount;

            _buffers = new Stack<byte[]>(baseBufferCount);

            for (int i = 0; i < baseBufferCount; i++)
            {
                _buffers.Push(new byte[baseBufferLength]);
            }
        }

        /// <summary>
        /// 获取一个缓冲块。如果缓冲池中有多余的，则返回；如果缓冲池中没有多余的，则额外分配一个新缓冲池
        /// </summary>
        /// <para>用此方法获得的缓冲块 应当使用 <see>ReturnBuffer</see> 返还到缓冲池中</para>
        /// <returns>缓冲块</returns>
        public byte[] GetBuffer()
        {
            lock (_buffers)
            {
                if (_buffers.Count > 0)
                {
                    byte[] buffer = _buffers.Pop();
                    return buffer;
                }
            }
            return AllocateNewBuffer();
        }

        private byte[] AllocateNewBuffer()
        {
            byte[] newBuffer = new byte[_bufferLength];
            _bufferCount++;
            OnNewBufferAllocated(EventArgs.Empty);

            return newBuffer;
        }

        /// <summary>
        /// 返还指定的 缓冲块 到 缓冲池 中
        /// </summary>
        /// <param name="buffer">需要释放的缓冲块</param>
        /// <returns>如果该缓冲块属于本池，则返回true；否则返回false</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ReturnBuffer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length != _bufferLength) // TODO: 仅靠块大小进行所属判断，是否有不安全性
                return false;

            if (ClearOnReturn)
                Array.Clear(buffer, 0, buffer.Length);

            lock (_buffers)
            {
                if (!_buffers.Contains(buffer))
                    _buffers.Push(buffer);
            }
            return true;
        }

        /// <summary>
        /// 为缓冲池添加指定数量的缓冲块
        /// </summary>
        /// <param name="buffersToAdd">需要添加的缓冲块个数</param>
        /// <para>注意：该函数不会发出<see>NewBufferAllocated</see>事件</para>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void IncreaseBufferCount(int buffersToAdd)
        {
            if (buffersToAdd <= 0)
                throw new ArgumentOutOfRangeException("buffersToAdd", buffersToAdd, "The number of buffers to add must be a nonnegative, nonzero integer.");

            List<byte[]> newBuffers = new List<byte[]>(buffersToAdd);
            for (int i = 0; i < buffersToAdd; i++)
            {
                newBuffers.Add(new byte[_bufferLength]);
            }

            lock (_buffers)
            {
                _bufferCount += buffersToAdd;
                for (int i = 0; i < buffersToAdd; i++)
                {
                    _buffers.Push(newBuffers[i]);
                }
            }
        }

        /// <summary>
        /// 从缓冲池中移除指定数量的缓冲块
        /// </summary>
        /// <param name="buffersToRemove">期望移除的缓冲块数量</param>
        /// <returns>实际移除的缓冲块数量</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int DecreaseBufferCount(int buffersToRemove)
        {
            if (buffersToRemove <= 0)
                throw new ArgumentOutOfRangeException("buffersToRemove", buffersToRemove, "The number of buffers to remove must be a nonnegative, nonzero integer.");

            int numRemoved = 0;
            lock (_buffers)
            {
                for (int i = 0; i < buffersToRemove && _buffers.Count > 0; i++)
                {
                    _buffers.Pop();
                    numRemoved++;
                    _bufferCount--;
                }
            }
            return numRemoved;
        }
    }
}
