using NAudio.Wave;
using SplitStreamLib.Internal;
using SplitStreamLib.Recording.Internal;
using System;
using System.Collections.Generic;
using System.IO;

namespace SplitStreamLib.Recording
{
    public class Recorder
    {
        private readonly RecorderOptions _options;

        private BufferedWaveProvider _waveProvider = null;
        private IMp3FrameDecompressor _decompressor = null;
        private ISampleProvider _sampleProvider = null;
        private int _channels = 0;

        private readonly Trigger _startTrigger = null;
        private readonly Trigger _endTrigger = null;
        private readonly FrameInfoBuffer _buffer = null;

        public Recorder(RecorderOptions options)
        {
            _options = options;
           
            _startTrigger = new Trigger(options.TriggerVolume, options.TriggerDuration, true);
            _endTrigger = new Trigger(options.EndTriggerVolume, options.EndTriggerDuration, false);

            _buffer = new FrameInfoBuffer(options.MinClipSeparation);
        }

        public void ProcessFrames(IEnumerable<Mp3Frame> frames)
        {
            var decompressedData = new byte[1024 * 16];
            var bytesDecompressed = 0;

            var samples = new float[1024 * 4];
            var currentTrigger = _startTrigger;

            var isRecording = false;

            Stream outputFile = null;
            string outputFileName = string.Empty;
            string fullOutputFileName = string.Empty;
            foreach (var frame in frames)
            {
                // find the average volume for the samples in this frame.
                bytesDecompressed = DecompressFrame(frame, decompressedData);
                // convert samples to wave
                _waveProvider.AddSamples(decompressedData, 0, bytesDecompressed);
                var samplesRead = _sampleProvider.Read(samples, 0, _waveProvider.BufferedBytes);
                // find average of samples
                var average = GetSamplesAverage(samples, samplesRead);

                var frameInfo = new Mp3FrameInfo(frame, average);

                currentTrigger.AddFrameInfo(frameInfo);
                if(currentTrigger.IsSet())
                {
                    // we need to switch state
                    if(isRecording)
                    {
                        // the end recording trigger was set
                        isRecording = false; 

                        _buffer.Clear();

                        _startTrigger.Clear();
                        currentTrigger = _startTrigger;

                        outputFile.Flush();
                        outputFile.Close();
                    }
                    else
                    {
                        // the start recording trigger was set
                        isRecording = true;

                        _endTrigger.Clear();
                        currentTrigger = _endTrigger;

                        if(!_buffer.IsFull && !string.IsNullOrEmpty(outputFileName))
                        {
                            // since we had a previous file and the buffer isn't full yet we'll just append
                            // this recording to the previous one.

                            outputFile = File.OpenWrite(fullOutputFileName);

                            Console.WriteLine($"Appneding clip {outputFileName}");

                            // the trigger frames should be contained in the buffer along with any
                            // frames since the previous recording stopped.
                            foreach (var fi in _buffer.GetFrames())
                            {
                                var d = fi.Frame.RawData;
                                outputFile.Write(d, 0, d.Length);
                            }
                        }
                        else
                        {
                            var fileTime = DateTime.Now;
                            outputFileName = fileTime.ToString("yyyy.MM.dd.Thh.mm.ss.ffffff") + ".mp3";
                            fullOutputFileName = Path.Combine(GetFilePath(fileTime), outputFileName);
                            outputFile = File.OpenWrite(fullOutputFileName);

                            Console.WriteLine($"Writing clip {outputFileName}");

                            // We want to put the frames from the trigger into the output
                            foreach (var fi in _startTrigger.GetFrames())
                            {
                                var d = fi.Frame.RawData;
                                outputFile.Write(d, 0, d.Length);
                            }
                        }


                    }
                }

                if(!isRecording)
                {
                    _buffer.Enqueue(frameInfo);
                }
                else
                {
                    // write the frame to the file.
                    var data = frameInfo.Frame.RawData;
                    outputFile.Write(data, 0, data.Length);
                }
            }
        }

        private string GetFilePath(DateTime fileTime)
        {
            if (!Directory.Exists(_options.OutputDirectory))
                Directory.CreateDirectory(_options.OutputDirectory);

            var day = fileTime.ToString("yyyy.MM.dd");
            var hour = fileTime.ToString("hh");
            var path = Path.Combine(_options.OutputDirectory, day)
;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, hour);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        private double GetSamplesAverage(float[] samples, int samplesRead)
        {
            float maxSample = 0f;

            float subAverage = 0f;
            int subCount = 0;
            float totalAverage = 0f;
            int totalCount = 0;
            float average = 0;

            int subCountLimit = 25;
            for (var index = 0; index < samplesRead; index += _channels)
            {
                maxSample = 0;
                for (var channel = 0; channel < _channels; channel++)
                {
                    float sampleValue = Math.Abs(samples[index + channel]);
                    maxSample = Math.Max(maxSample, sampleValue);
                }

                subAverage += maxSample;
                subCount++;

                if (subCount >= subCountLimit)
                {
                    totalAverage += subAverage / subCount;
                    totalCount++;

                    subAverage = 0f;
                    subCount = 0;
                }
            }

            average = totalAverage / totalCount;

            // convert volume to amplitude percentage
            double MinDb = -60.0;
            double MaxDb = 3.0;
            double db = 20 * Math.Log10(average);
            if (db < MinDb)
                db = MinDb;
            if (db > MaxDb)
                db = MaxDb;
            double percent = (db - MinDb) / (MaxDb - MinDb);
            if (double.IsNaN(percent))
                return 0d;

            if(_options.Debug)
                Console.WriteLine($"avg: {average}, volume percent: {percent}");

            return percent;
        }

        private int DecompressFrame(Mp3Frame frame, byte[] buffer)
        {
            // decode frame
            if (_decompressor == null)
            {
                WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate);
                _decompressor = new AcmMp3FrameDecompressor(waveFormat);

                _waveProvider = new BufferedWaveProvider(_decompressor.OutputFormat);
                _waveProvider.BufferDuration = TimeSpan.FromSeconds(5);

                _channels = _waveProvider.WaveFormat.Channels;
                
                _sampleProvider = _waveProvider.ToSampleProvider();
            }

            return _decompressor.DecompressFrame(frame, buffer, 0);
        }

    }
}
