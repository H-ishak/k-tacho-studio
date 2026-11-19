using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using BitFab.KW1281Test.Interface;

namespace KTachoStudio.Core.Hardware;

/// <summary>
/// Precise in-memory KW1281 protocol simulator for offline bench testing and automated tests.
/// Accurately models the byte-by-byte half-duplex complement protocol exchange of VW EDC15 and VDO Cluster.
/// </summary>
public sealed class SimulatedInterface : IInterface
{
    private readonly ConcurrentQueue<byte> _rxQueue = new();
    private readonly AutoResetEvent _dataAvailable = new(false);

    private byte _targetAddress = 0x01;
    private byte _blockCounter = 1;

    private readonly Queue<List<byte>> _pendingBlocks = new();
    private List<byte>? _currentBlock = null;
    private int _currentBlockIndex = 0;
    private byte? _ecuByteAwaitingAck = null;

    // 0 = idle/expecting block length, > 0 = payload bytes to ACK, -1 = expecting 0x03 block end
    private int _testerBlockState = 0;

    public int ReadTimeout { get; set; } = 4000;
    public int WriteTimeout { get; set; } = 4000;

    public byte ReadByte()
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(ReadTimeout);
        while (DateTime.UtcNow < deadline)
        {
            if (_rxQueue.TryDequeue(out var b))
            {
                return b;
            }
            _dataAvailable.WaitOne(50);
        }
        throw new TimeoutException("Simulated K-line timeout waiting for byte.");
    }

    public void WriteByteRaw(byte b)
    {
        // 1. Single-wire physical transceiver echo
        _rxQueue.Enqueue(b);
        _dataAvailable.Set();

        // 2. 5-baud Wakeup handshake completion
        if (b == 0x75) // Inverted KW2 (0x8A)
        {
            InitSimulatedBlocks();
            SendNextEcuByte();
            return;
        }

        // 3. KW1281 Protocol State Machine
        if (_ecuByteAwaitingAck.HasValue)
        {
            // Tester sent complement ACK for the byte ECU previously transmitted
            _ecuByteAwaitingAck = null;
            SendNextEcuByte();
        }
        else
        {
            // Tester is transmitting a block to the ECU (ACK block, EndCommunication, etc.)
            if (_testerBlockState == 0)
            {
                // First byte of tester block: length L
                _testerBlockState = b - 1; // Remaining bytes to complement ACK
                _rxQueue.Enqueue((byte)~b);
                _dataAvailable.Set();
            }
            else if (_testerBlockState > 0)
            {
                _testerBlockState--;
                if (_testerBlockState == 0)
                {
                    _testerBlockState = -1; // Next byte must be block end 0x03
                }
                _rxQueue.Enqueue((byte)~b);
                _dataAvailable.Set();
            }
            else if (_testerBlockState == -1)
            {
                // Tester sent block end (0x03)
                _testerBlockState = 0;
                if (_pendingBlocks.Count > 0)
                {
                    SendNextEcuByte();
                }
            }
        }
    }

    public void WriteBytesRaw(byte[] bytes)
    {
        foreach (var b in bytes)
        {
            WriteByteRaw(b);
        }
    }

    public void ReadBytes(byte[] buffer, int count)
    {
        for (var i = 0; i < count; i++)
        {
            buffer[i] = ReadByte();
        }
    }

    public void ClearReceiveBuffer()
    {
        while (_rxQueue.TryDequeue(out _)) { }

        // Controller wakes up and provides sync byte + keywords
        _blockCounter = 1;
        _rxQueue.Enqueue(0x55);
        _rxQueue.Enqueue(0x01);
        _rxQueue.Enqueue(0x8A);
        _dataAvailable.Set();
    }

    public void SetBreak(bool on) { }
    public void SetBaudRate(int baudRate) { }
    public void SetParity(Parity parity) { }
    public void SetDtr(bool on) { }
    public void SetRts(bool on) { }

    public void SetTargetAddress(byte address)
    {
        _targetAddress = address;
    }

    public void Dispose()
    {
        _dataAvailable.Dispose();
    }

    private void InitSimulatedBlocks()
    {
        _pendingBlocks.Clear();
        _currentBlock = null;
        _currentBlockIndex = 0;
        _ecuByteAwaitingAck = null;
        _testerBlockState = 0;

        if (_targetAddress == 0x17) // Cluster VDO
        {
            AddBlock(0xF6, Encoding.ASCII.GetBytes("1J0920826C  KOMBI+WEGFAHRS. VDO V08"));
            AddBlock(0xF6, [0x00, 0x00, 0x04, 0x04, 0xD2]); // Coding 00002, WSC 01234
            AddBlock(0x09, []); // ACK end
        }
        else // EDC15 Engine ECU (0x01)
        {
            AddBlock(0xF6, Encoding.ASCII.GetBytes("038906019HJ  1.9l R4 EDC 0000SG  1536"));
            AddBlock(0xF6, [0x00, 0x00, 0x04, 0x04, 0xD2]); // Coding 00002, WSC 01234
            AddBlock(0x09, []); // ACK end
        }
    }

    private void AddBlock(byte title, byte[] body)
    {
        var counter = _blockCounter;
        _blockCounter += 2; // Tester will consume the intermediate counter for its ACK block

        var block = new List<byte>
        {
            (byte)(body.Length + 3), // Block Length
            counter,                 // Block Counter
            title                    // Block Title
        };
        block.AddRange(body);
        block.Add(0x03);             // Block End
        _pendingBlocks.Enqueue(block);
    }

    private void SendNextEcuByte()
    {
        if (_currentBlock == null || _currentBlockIndex >= _currentBlock.Count)
        {
            if (_pendingBlocks.Count == 0)
                return;

            _currentBlock = _pendingBlocks.Dequeue();
            _currentBlockIndex = 0;
        }

        var b = _currentBlock[_currentBlockIndex++];
        _rxQueue.Enqueue(b);
        _dataAvailable.Set();

        // The block end (last byte in block) is not ACK'd by tester. Any other byte is ACK'd with ~b.
        var isBlockEnd = (_currentBlockIndex >= _currentBlock.Count);
        _ecuByteAwaitingAck = isBlockEnd ? null : b;
    }
}
