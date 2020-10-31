using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureProgramming3.Services
{
    public interface IParallelReaderService
    {
        void Initiate();
        Task<int> AddThread();
        Task<int> RemoveThread();
    }
}
