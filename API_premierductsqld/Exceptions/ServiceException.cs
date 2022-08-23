﻿using System;
namespace API_premierductsqld.Exceptions
{
    public class ServiceException : Exception
    {
        public string error { get; set; }
        public string description { get; set; }
 
        public ServiceException(string error): base(error)
        {
        }
    }
}
