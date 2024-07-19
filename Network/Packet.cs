namespace Network
{
    public enum ServerPackets
    {
        Handshake = 0,
        Error = 1,
        Message = 2,
        Account = 3,
        SuccessfulLogin = 4,
        Role = 5,
        RoleDelete = 6,
        AccountDelete = 7,
        Document = 8,
        DocumentDelete = 9,
        FilePartRequest = 10,
        DownloadFileResponse = 11,
        FilePart = 12,
    }
    public enum ClientPackets
    {
        Handshake = 0,
        AccountListRequest = 1,
        RoleListRequest = 2,
        ChangeUsernameRequest = 3,
        DocumentsListRequest = 4,
        File = 5,
        FilePart = 6,
        FileDeleteRequest = 7,
        DownloadFileRequest = 8,
        FilePartRequest = 9,
        AddUserRole= 10,
        RemoveUserRole = 11,
    }
    public class Packet : IDisposable
    {
        List<byte> buffer;
        byte[] readableBuffer;
        int readPos;

        private bool disposed = false;

        public Packet()
        {
            buffer = new List<byte>();
            readPos = 0;
        }
        public Packet(int id)
        {
            buffer = new List<byte>();
            readPos = 0;
            Write(id);
        }
        public Packet(byte[] _data)
        {
            buffer = new List<byte>();
            readPos = 0;

            SetBytes(_data);
        }
        public void WriteLenght()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }

        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = buffer.ToArray();
        }

        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public int Length()
        {
            return buffer.Count;
        }

        public int UnreadLenght()
        {
            return Length() - readPos;
        }

        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            }
            else
            {
                readPos -= 4;
            }
        }
        #region Write Data
        public void Write(byte value)
        {
            buffer.Add(value);
        }

        public void Write(byte[] value)
        {
            buffer.AddRange(value);
        }

        public void Write(int value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(uint value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(long value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(float value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(bool value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes.Length);
            buffer.AddRange(bytes);
        }
        #endregion

        #region Read Data
        public byte ReadByte(bool moveReadPos = true)
        {
            if(buffer.Count > readPos)
            {
                byte value = readableBuffer[readPos];
                if (moveReadPos)
                {
                    readPos++;
                }
                return value;
            }
            else
            {
                throw new Exception("Couldnt read value");
            }
        }

        public byte[] ReadBytes(int lenght, bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                byte[] value = buffer.GetRange(readPos, lenght).ToArray();
                if (moveReadPos)
                {
                    readPos += lenght;
                }
                return value;
            }
            else
            {
                throw new Exception("Couldnt read value");
            }
        }

        public int ReadInt(bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                int value = BitConverter.ToInt32(readableBuffer, readPos);
                if (moveReadPos)
                {
                    readPos+=4;
                }
                return value;
            }
            else
            {
                throw new Exception("Couldnt read value");
            }
        }

        public uint ReadUInt(bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                uint value = BitConverter.ToUInt32(readableBuffer, readPos);
                if (moveReadPos)
                {
                    readPos += 4;
                }
                return value;
            }
            else
            {
                throw new Exception("Couldnt read value");
            }
        }

        public long ReadLong(bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                long value = BitConverter.ToInt64(readableBuffer, readPos);
                if (moveReadPos)
                {
                    readPos += 4;
                }
                return value;
            }
            else
            {
                throw new Exception("Couldnt read value");
            }
        }

        public float ReadFloat(bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                float value = BitConverter.ToSingle(readableBuffer, readPos);
                if (moveReadPos)
                {
                    readPos += 4;
                }
                return value;
            }
            else
            {
                throw new Exception("Couldnt read value");
            }
        }

        public bool ReadBool(bool moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                bool value = BitConverter.ToBoolean(readableBuffer, readPos);
                if (moveReadPos)
                {
                    readPos += 1;
                }
                return value;
            }
            else
            {
                throw new Exception("Couldnt read value");
            }
        }

        public string ReadString(bool moveReadPos = true)
        {
            int lenght = ReadInt();
            string value = Encoding.UTF8.GetString(readableBuffer,readPos, lenght);
            if (moveReadPos && value.Length > 0)
            {
                readPos += lenght;
            }
            return value;
        }
        #endregion
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    buffer.Clear();
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }
                disposed = true;
            }
        }
    }
}
