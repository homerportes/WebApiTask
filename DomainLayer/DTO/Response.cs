﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.DTO
{
    public class Response
    {
        public bool ThrereIsError => Errors.Any();
        public long EntityId { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>(0);
    }
    public class Response<T> : Response where T : class
    {
        public IEnumerable<T> DataList { get; set; }
        public T SingleData { get; set; }
    }

}
