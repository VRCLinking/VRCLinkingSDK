/*
using System;
using UdonSharp;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;
using Debug = UnityEngine.Debug;

public class LZWCompressor : UdonSharpBehaviour
{
    private const int MAX_BITS = 14; //maimxum bits allowed to read
    private const int HASH_BIT = MAX_BITS - 8; //hash bit to use with the hasing algorithm to find correct index
    private const int MAX_VALUE = (1 << MAX_BITS) - 1; //max value allowed based on max bits
    private const int MAX_CODE = MAX_VALUE - 1; //max code possible
    private const int TABLE_SIZE = 18041; //must be bigger than the maximum allowed by maxbits and prime

    private int[] _iaCodeTable = new int[TABLE_SIZE]; //code table
    private int[] _iaPrefixTable = new int[TABLE_SIZE]; //prefix table
    private int[] _iaCharTable = new int[TABLE_SIZE]; //character table

    private ulong _iBitBuffer; //bit buffer to temporarily store bytes read from the files
    private int _iBitCounter; //counter for knowing how many bits are in the bit buffer
    int _index = 0;
    int _readerLength;
    byte[] _reader;
    byte[] _writer;
    int _indexWriter = 0;
    int _maxIndexUntilExpand = 0;
    IUdonEventReceiver _receiver;
    string _eventName;

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

            bChar = baDecodeStack[iCounter]; //set last char used
            
            int iCounterPlusOne = iCounter + 1;
            Buffer.BlockCopy(baDecodeStack, 0, _writer, _indexWriter, iCounterPlusOne);
            _indexWriter += iCounterPlusOne;

            if (iNextCode <= MAX_CODE) //insert into tables
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

    void UpdateWriter()
    {
        var writerBytes = new byte[_indexWriter];
        Buffer.BlockCopy(_writer, 0, writerBytes, 0, _indexWriter);
        
        _writer = writerBytes;
    }
}
*/