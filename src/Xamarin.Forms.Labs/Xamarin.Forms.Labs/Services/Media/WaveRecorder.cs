﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Labs.Services.Media
{
    public class WaveRecorder
    {
        //private StreamWriter streamWriter;
        private BinaryWriter writer;
        private int byteCount;
        private IAudioStream stream;

        ~WaveRecorder()
        {
            StopRecorder().Wait();
        }

        public async Task<bool> StartRecorder(IAudioStream stream, Stream fileStream, int sampleRate)
        {
            if (this.stream != null || stream == null)
            {
                return false;
            }

            this.stream = stream;

            try
            {
                this.writer = new BinaryWriter(fileStream, Encoding.UTF8);
            }
            catch (Exception)
            {
                return false;
            }

            this.byteCount = 0;
            this.stream.OnBroadcast += OnStreamBroadcast;

            return await this.stream.Start(sampleRate);
        }

        public async Task StopRecorder()
        {
            if (this.stream != null)
            {
                this.stream.OnBroadcast -= OnStreamBroadcast;
                await this.stream.Stop();
            }
            if (this.writer != null && this.writer.BaseStream.CanWrite)
            {
                this.WriteHeader();
                this.writer.Dispose();
                this.writer = null;
            }

            this.stream = null;
        }

        private void OnStreamBroadcast(object sender, EventArgs<byte[]> eventArgs)
        {
            this.writer.Write(eventArgs.Value);
            this.byteCount += eventArgs.Value.Length;
        }


        private void WriteHeader()
        {
            this.writer.Seek(0, SeekOrigin.Begin);
            // chunk ID
            this.writer.Write('R');
            this.writer.Write('I');
            this.writer.Write('F');
            this.writer.Write('F');

            this.writer.Write(this.byteCount + 36);
            this.writer.Write('W');
            this.writer.Write('A');
            this.writer.Write('V');
            this.writer.Write('E');

            this.writer.Write('f');
            this.writer.Write('m');
            this.writer.Write('t');
            this.writer.Write(' ');

            this.writer.Write(16);
            this.writer.Write((short)1);

            this.writer.Write((short)this.stream.ChannelCount);
            this.writer.Write(this.stream.SampleRate);
            this.writer.Write(this.stream.SampleRate * 2);
            this.writer.Write((short)2);
            this.writer.Write((short)this.stream.BitsPerSample);
            this.writer.Write('d');
            this.writer.Write('a');
            this.writer.Write('t');
            this.writer.Write('a');
            this.writer.Write(this.byteCount);
        }
    }
}
