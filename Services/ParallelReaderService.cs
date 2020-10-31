﻿using Microsoft.Extensions.Logging;
using SecureProgramming3.Helpers;
using SecureProgramming3.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SecureProgramming3.Services
{
    public class ParallelReaderService : IParallelReaderService
    {
        private readonly Channel<ChannelData> _channel;
        private readonly ConcurrentQueue<StreamData> _documents;
        private readonly ILogger<IParallelReaderService> _logger;
        private readonly Dictionary<CancellationTokenSource, ConsumerProducer> _threads;

        const string FilePaths = @"C:\_repositories\SecureProgramming3\rand_files\";
        const int DelayTime = 10;

        private int _totalFilesDone = 0;
        private int _maxPrime = 0;
        private int _minPrime = Int32.MaxValue;

        private static object _totalFilesDoneSynchronizationObject = new Object();
        private static object _maxSynchronizationObject = new Object();
        private static object _minSynchronizationObject = new Object();

        public ParallelReaderService(ILogger<ParallelReaderService> logger)
        {
            _threads = new Dictionary<CancellationTokenSource, ConsumerProducer>();
            _documents = new ConcurrentQueue<StreamData>();
            _channel = Channel.CreateBounded<ChannelData>(Int32.MaxValue);
            _logger = logger;
        }

        public void Initiate()
        {
            string[] filePaths = Directory.GetFiles(FilePaths);
            foreach(var filePath in filePaths)
            {
                var file = new System.IO.StreamReader(filePath);
                var fileName = filePath.Substring(FilePaths.Length);

                _documents.Enqueue(new StreamData { Stream = file, FileName = fileName});
            }
        }

        public async Task<int> AddThread()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Task producer = Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    if (_documents.TryDequeue(out var file))
                    {
                        var numbers = new List<int>();

                        string line;
                        while ((line = file.Stream.ReadLine()) != null)
                        {
                            numbers.Add(Int32.Parse(line));

                            //Otherwise all the files are checked too fast
                            await Task.Delay(DelayTime);
                        }

                        _channel.Writer.TryWrite(new ChannelData() { FileName = file.FileName, FileData = numbers });
                    }
                    else
                    {
                        _channel.Writer.TryComplete();
                    }
                }
            }, token);

            Task consumer = Task.Run(async () => {
                while (await _channel.Reader.WaitToReadAsync() && !token.IsCancellationRequested)
                {
                    if (_channel.Reader.TryRead(out var file))
                    {
                        var count = file.FileData.Count;
                        foreach(var number in file.FileData)
                        {
                            var isPrime = PrimeHelper.IsPrime(number);
                            if (isPrime)
                            {
                                _logger.LogInformation(file.FileName + " : " + number.ToString() + " : " + count);

                                if (_minPrime > number) ChangeMin(number);
                                if (_maxPrime < number) ChangeMax(number);
                            }

                            //Otherwise all the files are checked too fast
                            await Task.Delay(DelayTime);
                            count--;
                        }
                    }
                    IncrementTotalFilesDone();
                }
            }, token);

            var consumerProducer = new ConsumerProducer { Producer = producer, Consumer = consumer };
            _threads.Add(source, consumerProducer);

            return await Task.FromResult(_threads.Count);
        }

        public async Task<int> RemoveThread()
        {
            if(_threads.Count > 0)
            {
                var thread = _threads.Last();

                thread.Key.Cancel();
                Task.WaitAll(thread.Value.Producer, thread.Value.Consumer);

                _threads.Remove(thread.Key);
            }

            return await Task.FromResult(_threads.Count);
        }

        private void IncrementTotalFilesDone()
        {
            lock (_totalFilesDoneSynchronizationObject)
            {
                _totalFilesDone++;
                _logger.LogInformation("Total files done: " + _totalFilesDone);
            }
        }

        private void ChangeMax(int value)
        {
            lock (_maxSynchronizationObject)
            {
                _maxPrime = value;
                _logger.LogInformation("New MAX: " + value);
            }
        }

        private void ChangeMin(int value)
        {
            lock (_minSynchronizationObject)
            {
                _minPrime = value;
                _logger.LogInformation("New MIN: " + value);
            }
        }
    }
}
