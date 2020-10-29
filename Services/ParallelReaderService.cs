using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureProgramming3.Services
{
    public class ParallelReaderService : IParallelReaderService
    {
        private int _threads;
        public ParallelReaderService()
        {
            _threads = 0;
        }

        public bool IsInitated
        {
            get
            {
                return _threads > 0;
            }
        }

        public async Task<int> AddThread()
        {
            _threads++;

            return await Task.FromResult(_threads);
        }

        public async Task<int> RemoveThread()
        {
            _threads--;

            return await Task.FromResult(_threads);
        }
    }
}
