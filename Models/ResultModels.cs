using System;
using System.Collections.Generic;

namespace BSDN_API.Models
{
    public class ModelResult<T>
    {
        public int Status { set; get; }
        public string Message { set; get; }
        public T Data { set; get; }

        public ModelResult(int status, T data, string message)
        {
            Status = status;
            Data = data;
            Message = message;
        }
    }

    public class ModelResultList<T>
    {
        public int Status { set; get; }
        public string Message { set; get; }
        public List<T> Data { set; get; }
        public bool HasNext { set; get; }
        public int TotalCount { set; get; }

        public ModelResultList(int status, List<T> data, string message, bool hasNext, int count)
        {
            Status = status;
            Data = data;
            Message = message;
            HasNext = hasNext;
            TotalCount = count;
        }
    }
}