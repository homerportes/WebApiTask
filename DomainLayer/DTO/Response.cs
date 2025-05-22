using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainLayer.DTO
{
    using System.Collections.Generic;
    using System.Linq;

    public class Response
    {
        public bool ThrereIsError => Errors.Any();
        public long EntityId { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }

    public class Response<T> : Response where T : class
    {
        public IEnumerable<T> DataList { get; set; } = Enumerable.Empty<T>();
        public T? SingleData { get; set; }
        public Dictionary<string, object> CalculosExtra { get; set; } = new();
    }
}
