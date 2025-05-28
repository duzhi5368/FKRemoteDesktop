using FKRemoteDesktop.Message.MessageStructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Utilities
{
    public class FileSplit : IEnumerable<FileChunk>, IDisposable
    {
        private readonly FileStream _fileStream;        // 当前打开的文件
        public readonly int MaxChunkSize = 65535;       // 每个文件块的最大大小
        public string FilePath => _fileStream.Name;     // 打开的文件名
        public long FileSize => _fileStream.Length;     // 打开的文件大小

        public FileSplit(string filePath, FileAccess fileAccess)
        {
            switch (fileAccess)
            {
                case FileAccess.Read:
                    _fileStream = File.OpenRead(filePath);
                    break;
                case FileAccess.Write:
                    _fileStream = File.OpenWrite(filePath);
                    break;
                default:
                    throw new ArgumentException($"{nameof(fileAccess)} 必须时进行 读操作 或 写操作.");
            }
        }

        // 将一个文件块写入文件中
        public void WriteChunk(FileChunk chunk)
        {
            _fileStream.Seek(chunk.Offset, SeekOrigin.Begin);
            _fileStream.Write(chunk.Data, 0, chunk.Data.Length);
        }

        // 读取一个文件块
        public FileChunk ReadChunk(long offset)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);

            long chunkSize = _fileStream.Length - _fileStream.Position < MaxChunkSize
                ? _fileStream.Length - _fileStream.Position
                : MaxChunkSize;

            var chunkData = new byte[chunkSize];
            _fileStream.Read(chunkData, 0, chunkData.Length);

            return new FileChunk
            {
                Data = chunkData,
                Offset = _fileStream.Position - chunkData.Length
            };
        }

        public IEnumerator<FileChunk> GetEnumerator()
        {
            for (long currentChunk = 0; currentChunk <= _fileStream.Length / MaxChunkSize; currentChunk++)
            {
                yield return ReadChunk(currentChunk * MaxChunkSize);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileStream.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
