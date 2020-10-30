using Microsoft.Extensions.Logging;
using SecureProgramming3.Helpers;
using SecureProgramming3.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SecureProgramming3.Services
{
    public class ParallelReaderService : IParallelReaderService
    {
        private readonly Channel<ChannelData> _channel;
        private readonly ConcurrentQueue<StreamReader> _documents;
        private readonly ILogger<IParallelReaderService> _logger;
        private readonly List<ConsumerProducer> _threads;
        
        public ParallelReaderService(ILogger<ParallelReaderService> logger)
        {
            _threads = new List<ConsumerProducer>();
            _documents = new ConcurrentQueue<StreamReader>();
            _channel = Channel.CreateBounded<ChannelData>(Int32.MaxValue);
            _logger = logger;
        }

        public void Initiate()
        {
            string[] filePaths = Directory.GetFiles(@"C:\_repositories\SecureProgramming3\rand_files\");
            foreach(var filePath in filePaths)
            {
                var file = new System.IO.StreamReader(filePath);
                _documents.Enqueue(file);
            }
        }

        public bool IsInitated
        {
            get
            {
                return _threads.Count > 0;
            }
        }

        public async Task<int> AddThread()
        {
            Task producer = Task.Factory.StartNew(() => {
                if(_documents.TryDequeue(out var file))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        var channelData = new ChannelData() { number = Int32.Parse(line) };
                        _channel.Writer.TryWrite(channelData);
                    }
                }
                _channel.Writer.Complete();
            });

            Task Consumer = Task.Factory.StartNew(async () => {
                while (await _channel.Reader.WaitToReadAsync())
                {
                    if (_channel.Reader.TryRead(out var channelData))
                    {
                        var isPrime = PrimeHelper.IsPrime(channelData.number);
                        if (isPrime)
                        {
                            _logger.LogInformation(channelData.number.ToString());
                        }
                    }
                }
            });

            var thread = new ConsumerProducer { Producer = producer, Consumer = Consumer };
            _threads.Add(thread);

            return await Task.FromResult(_threads.Count);
        }

        public async Task<int> RemoveThread()
        {
            if(_threads.Count > 0)
            {
                var thread = _threads.Last();

                Task.WaitAll(thread.Consumer, thread.Producer);
                _threads.Remove(thread);
            }

            return await Task.FromResult(_threads.Count);
        }
    }
}
