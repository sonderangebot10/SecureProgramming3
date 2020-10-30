using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureProgramming3.Models
{
    public class ConsumerProducer
    {
        public Task Consumer { get; set; }
        public Task Producer { get; set; }
    }
}
