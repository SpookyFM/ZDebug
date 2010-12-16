﻿using System;
using System.Collections.Generic;
using System.IO;
using ZDebug.Core.Basics;

namespace ZDebug.Core.Blorb
{
    public sealed class BlorbFile
    {
        private readonly Memory memory;
        private int releaseNumber;

        private static string NameFromId(uint id)
        {
            var chars = new char[4];
            chars[0] = (char)((id & 0xff000000) >> 24);
            chars[1] = (char)((id & 0x00ff0000) >> 16);
            chars[2] = (char)((id & 0x0000ff00) >> 8);
            chars[3] = (char)(id & 0x000000ff);

            return new string(chars);
        }

        private static uint MakeId(char c1, char c2, char c3, char c4)
        {
            return ((uint)((byte)c1 << 24) | (uint)((byte)c2 << 16) | (uint)((byte)c3 << 8) | (byte)c4);
        }

        private static readonly uint id_FORM = MakeId('F', 'O', 'R', 'M');
        private static readonly uint id_IFRS = MakeId('I', 'F', 'R', 'S');
        private static readonly uint id_RIdx = MakeId('R', 'I', 'd', 'x');
        private static readonly uint id_IFhd = MakeId('I', 'F', 'h', 'd');
        private static readonly uint id_Reso = MakeId('R', 'e', 's', 'o');
        private static readonly uint id_Loop = MakeId('L', 'o', 'o', 'p');
        private static readonly uint id_RelN = MakeId('R', 'e', 'l', 'N');
        private static readonly uint id_Plte = MakeId('P', 'l', 't', 'e');

        private static readonly uint id_Snd = MakeId('S', 'n', 'd', ' ');
        private static readonly uint id_Exec = MakeId('E', 'x', 'e', 'c');
        private static readonly uint id_Pict = MakeId('P', 'i', 'c', 't');
        private static readonly uint id_Copyright = MakeId('(', 'c', ')', ' ');
        private static readonly uint id_AUTH = MakeId('A', 'U', 'T', 'H');
        private static readonly uint id_ANNO = MakeId('A', 'N', 'N', 'O');

        private static readonly uint id_ZCOD = MakeId('Z', 'C', 'O', 'D');

        private struct ChunkDescriptor
        {
            public uint Type;
            public uint Length;
            public uint Address;
            public uint DataAddress;

            public override string ToString()
            {
                return string.Format("'{0}'; Address={1:x8}; DataAddress={2:x8}; Length={3}", NameFromId(Type), Address, DataAddress, Length);
            }
        }

        private struct ResourceDecriptor
        {
            public uint Usage;
            public uint Number;
            public int ChunkNumber;

            public override string ToString()
            {
                return string.Format("'{0}'; Number={1}; ChunkNumber", NameFromId(Usage), Number, ChunkNumber);
            }
        }

        private struct ZHeader
        {
            public ushort ReleaseNumber;
            public char[] SerialNumber;
            public ushort Checksum;
        }

        private readonly List<ChunkDescriptor> chunks;
        private readonly List<ResourceDecriptor> resources;

        public BlorbFile(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.memory = new Memory(stream);

            var reader = memory.CreateReader(0);

            var dwords = reader.NextDWords(3);

            // First, ensure that this is a valid format
            if (dwords[0] != id_FORM)
            {
                throw new InvalidOperationException();
            }

            if (dwords[2] != id_IFRS)
            {
                throw new InvalidOperationException();
            }

            int totalLength = (int)dwords[1] + 8;

            // Collect all chunks
            this.chunks = new List<ChunkDescriptor>();

            while (reader.Address < totalLength)
            {
                var chunk = new ChunkDescriptor();

                chunk.Address = (uint)reader.Address;

                var type = reader.NextDWord();
                var len = reader.NextDWord();

                chunk.Type = type;
                if (type == id_FORM)
                {
                    chunk.DataAddress = chunk.Address;
                    chunk.Length = len + 8;
                }
                else
                {
                    chunk.DataAddress = (uint)reader.Address;
                    chunk.Length = len;
                }

                chunks.Add(chunk);

                reader.Skip((int)len);
                if ((reader.Address & 1) != 0)
                {
                    reader.Skip(1);
                }

                if (reader.Address > totalLength)
                {
                    throw new InvalidOperationException();
                }
            }

            // Loop through chunks and collect resources
            this.resources = new List<ResourceDecriptor>();

            foreach (var chunk in chunks)
            {
                if (chunk.Type == id_RIdx)
                {
                    reader.Address = (int)chunk.DataAddress;
                    var numResources = (int)reader.NextDWord();

                    if (chunk.Length < (numResources * 12) + 4)
                    {
                        throw new InvalidOperationException();
                    }

                    for (int i = 0; i < numResources; i++)
                    {
                        var resource = new ResourceDecriptor();
                        resource.Usage = reader.NextDWord();
                        resource.Number = reader.NextDWord();

                        var resourcePos = reader.NextDWord();

                        var chunkIndex = chunks.FindIndex(c => c.Address == resourcePos);
                        if (chunkIndex < 0)
                        {
                            throw new InvalidOperationException();
                        }

                        resource.ChunkNumber = chunkIndex;

                        resources.Add(resource);
                    }
                }
                else if (chunk.Type == id_RelN)
                {
                    reader.Address = (int)chunk.DataAddress;
                    if (chunk.Length < 2)
                    {
                        throw new InvalidOperationException();
                    }

                    releaseNumber = reader.NextWord();
                }
                else if (chunk.Type == id_IFhd)
                {
                    reader.Address = (int)chunk.DataAddress;
                    if (chunk.Length < 3)
                    {
                        throw new InvalidOperationException();
                    }

                    var header = new ZHeader();
                    header.ReleaseNumber = reader.NextWord();
                    header.SerialNumber = new char[6];
                    for (int i = 0; i < 6; i++)
                    {
                        header.SerialNumber[i] = (char)reader.NextByte();
                    }
                    header.Checksum = reader.NextWord();
                }
                else if (chunk.Type == id_Reso)
                {

                }
                else if (chunk.Type == id_Loop)
                {

                }
                else if (chunk.Type == id_Plte)
                {

                }
            }
        }

        public byte[] GetZCode()
        {
            foreach (var chunk in chunks)
            {
                if (chunk.Type == id_ZCOD)
                {
                    return memory.ReadBytes((int)chunk.DataAddress, (int)chunk.Length);
                }
            }

            throw new InvalidOperationException();
        }

        public int ReleaseNumber
        {
            get { return releaseNumber; }
        }
    }
}
