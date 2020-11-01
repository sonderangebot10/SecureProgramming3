using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SecureProgramming3.Helpers;
using SecureProgramming3.Hubs;
using SecureProgramming3.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly IHubContext<PrimeHub> _primeHubContext;

        const int DelayTime = 3;

        private int _totalFilesDone = 0;
        private int _maxPrime = 0;
        private int _minPrime = Int32.MaxValue;

        private static object _totalFilesDoneSynchronizationObject = new Object();
        private static object _maxSynchronizationObject = new Object();
        private static object _minSynchronizationObject = new Object();

        private string FilePaths = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public ParallelReaderService(ILogger<ParallelReaderService> logger, IHubContext<PrimeHub> primeHubContext)
        {
            _threads = new Dictionary<CancellationTokenSource, ConsumerProducer>();
            _documents = new ConcurrentQueue<StreamData>();
            _channel = Channel.CreateBounded<ChannelData>(Int32.MaxValue);

            _logger = logger;
            _primeHubContext = primeHubContext;
        }

        public void Initiate()
        {
            var fullFilePaths = FilePaths + @"\\rand_files\";
            string[] filePaths = Directory.GetFiles(fullFilePaths);
            foreach(var filePath in filePaths)
            {
                var file = new System.IO.StreamReader(filePath);
                var fileName = filePath.Substring(fullFilePaths.Length);

                _documents.Enqueue(new StreamData { Stream = file, FileName = fileName});
            }
        }

        public async Task<int> AddThreadAsync()
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
                while (!token.IsCancellationRequested && await _channel.Reader.WaitToReadAsync())
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

                            ChangeFileCompleted(file.FileName, file.FileData.Count, count);
                        }
                    }
                    IncrementTotalFilesDone();
                }
            }, token);

            var consumerProducer = new ConsumerProducer { Producer = producer, Consumer = consumer };
            _threads.Add(source, consumerProducer);

            return await Task.FromResult(_threads.Count);
        }

        public async Task<int> RemoveThreadAsync()
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

        public async Task<int> GetThreadsAsync()
        {
            return await Task.FromResult(_threads.Count);
        }

        private void IncrementTotalFilesDone()
        {
            lock (_totalFilesDoneSynchronizationObject)
            {
                _totalFilesDone++;
                _logger.LogInformation("Total files done: " + _totalFilesDone);
                _primeHubContext.Clients.All.SendAsync("ChangeTotalFilesDone", _totalFilesDone.ToString()).ConfigureAwait(false);
            }
        }

        private void ChangeMax(int value)
        {
            lock (_maxSynchronizationObject)
            {
                _maxPrime = value;
                _logger.LogInformation("New maximum value: " + value);
                _primeHubContext.Clients.All.SendAsync("ChangeMax", value.ToString()).ConfigureAwait(false);
            }
        }

        private void ChangeMin(int value)
        {
            lock (_minSynchronizationObject)
            {
                _minPrime = value;
                _logger.LogInformation("New minimum value: " + value);
                _primeHubContext.Clients.All.SendAsync("ChangeMin", value.ToString()).ConfigureAwait(false);
            }
        }

        private void ChangeFileCompleted(string fileName, int total, int current)
        {
            lock (_minSynchronizationObject)
            {
                _primeHubContext.Clients.All.SendAsync("ChangeFile", fileName, total.ToString(), current.ToString()).ConfigureAwait(false);
            }
        }
    }
}
