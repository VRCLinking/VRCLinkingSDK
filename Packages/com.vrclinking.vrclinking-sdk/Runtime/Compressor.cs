﻿using System;
using System.Diagnostics;
using UdonSharp;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;
using Debug = UnityEngine.Debug;

namespace VRCLinking.Utilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Compressor : UdonSharpBehaviour
    {
        const int MAX_BITS = 14;
        const int HASH_BIT = MAX_BITS - 8;
        const int MAX_VALUE = (1 << MAX_BITS) - 1;
        const int MAX_CODE = MAX_VALUE - 1;
        const int TABLE_SIZE = 18041;

        int[] _iaCodeTable = new int[TABLE_SIZE];
        int[] _iaPrefixTable = new int[TABLE_SIZE];
        int[] _iaCharTable = new int[TABLE_SIZE];

        ulong _iBitBuffer;
        int _iBitCounter;
        int _index = 0;
        int _readerLength;
        byte[] _reader;
        byte[] _writer;
        int _indexWriter = 0;
        int _maxIndexUntilExpand = 0;
        IUdonEventReceiver _receiver;
        string _eventName;
        Stopwatch _frameTimer = new Stopwatch();

        void Initialize()
        {
            _iBitBuffer = 0;
            _iBitCounter = 0;
            _index = 0;
            _readerLength = _reader.Length;
            _writer = new byte[_reader.Length * 4];
            _indexWriter = 0;
            _maxIndexUntilExpand = _reader.Length * 3;

            iNextCode = 256;
            iNewCode = 0;
            iOldCode = 0;
            bChar = 0;
            iCurrentCode = 0;
            iCounter = 0;
            baDecodeStack = new byte[TABLE_SIZE];
            _frameTimer.Restart();
        }

        void ExpandWriter()
        {
            var newWriter = new byte[_writer.Length * 2];

            Buffer.BlockCopy(_writer, 0, newWriter, 0, _writer.Length);

            _writer = newWriter;
            _maxIndexUntilExpand = (int)(_writer.Length * 1.75f);
        }

        public void StartDecompression(byte[] inputBytes, IUdonEventReceiver receiver, string eventName)
        {
            _reader = inputBytes;
            _receiver = receiver;
            _eventName = eventName;

            Initialize();
            iOldCode = ReadCode();
            bChar = (byte)iOldCode;
            _writer[_indexWriter++] = (byte)iOldCode;

            iNewCode = ReadCode();

            DecompressionStep();
        }

        int iNextCode;
        int iNewCode;
        int iOldCode;
        byte bChar;
        int iCurrentCode;
        int iCounter;
        byte[] baDecodeStack;

        public void DecompressionStep()
        {
            _frameTimer.Restart();

            while (iNewCode != MAX_VALUE)
            {
                if (iNewCode >= iNextCode)
                {
                    baDecodeStack[0] = bChar;
                    iCounter = 1;
                    iCurrentCode = iOldCode;
                }
                else
                {
                    iCounter = 0;
                    iCurrentCode = iNewCode;
                }

                while (iCurrentCode > 255)
                {
                    baDecodeStack[iCounter] = (byte)_iaCharTable[iCurrentCode];
                    ++iCounter;
                    if (iCounter >= MAX_CODE)
                    {
                        Debug.LogError("Error: Counter is greater than or equal to MAX_CODE");
                        FinishDecompression();
                        return;
                    }

                    iCurrentCode = _iaPrefixTable[iCurrentCode];
                }

                baDecodeStack[iCounter] = (byte)iCurrentCode;

                bChar = baDecodeStack[iCounter];

                while (iCounter >= 0) 
                {
                    _writer[_indexWriter++] = baDecodeStack[iCounter];
                    --iCounter;
                }

                if (iNextCode <= MAX_CODE) 
                {
                    _iaPrefixTable[iNextCode] = iOldCode;
                    _iaCharTable[iNextCode] = bChar;
                    ++iNextCode;
                }

                iOldCode = iNewCode;

                iNewCode = ReadCode();

                if (_indexWriter >= _maxIndexUntilExpand)
                {
                    ExpandWriter();
                }

                if (_frameTimer.ElapsedMilliseconds > MaxFrameTime)
                {
                    _frameTimer.Stop();
                    SendCustomEventDelayedFrames(nameof(DecompressionStep), 1);
                    Debug.Log("Decompression step took too long, delaying next step");
                    return;
                }
            }

            FinishDecompression();
        }

        void FinishDecompression()
        {
            UpdateWriter();

            _receiver.SendCustomEvent(_eventName);
        }

        public byte[] GetDecompressedData()
        {
            return _writer;
        }

        uint _iReturnVal;
        const int BIT_SHIFT = 18;

        int ReadCode()
        {
            while (_iBitCounter <= 24) //fill up buffer
            {
                if (_index >= _readerLength)
                {
                    return MAX_VALUE;
                }

                _iBitBuffer |= (ulong)_reader[_index] << (24 - _iBitCounter); //insert byte into buffer
                _index += 2; //increment index
                _iBitCounter += 8; //increment counter
            }

            _iReturnVal = (uint)(_iBitBuffer & MaxUint) >> BIT_SHIFT; //(32 - MAX_BITS);
            _iBitBuffer <<= MAX_BITS; //remove it from buffer
            _iBitCounter -= MAX_BITS; //decrement bit counter

            if (_iReturnVal > MaxInt)
            {
                return int.MaxValue;
            }

            return (int)_iReturnVal;
        }

        const ulong MaxUint = uint.MaxValue;
        const uint MaxInt = int.MaxValue;
        const long MaxFrameTime = 5000;

        void UpdateWriter()
        {
            var writerBytes = new byte[_indexWriter];
            Buffer.BlockCopy(_writer, 0, writerBytes, 0, _indexWriter);

            _writer = writerBytes;
        }
    }
}