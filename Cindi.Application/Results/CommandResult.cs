using Cindi.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Results
{
    public class CommandResult
    {
        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                if (value != CommandResultTypes.Update && value != CommandResultTypes.Create && value != CommandResultTypes.None && value != CommandResultTypes.Delete)
                {
                    throw new InvalidCommandResultException(value);
                }
                _type = value;
            }
        }

        public CommandResult() { }
        public CommandResult(Exception e)
        {
            IsSuccessful = false;
            ErrorMessage = e.Message;
        }

        public long ElapsedMs { get; set; }
        public string ObjectRefId { get; set; }
        public bool IsSuccessful { get; set; } = true;
        public string ErrorMessage { get; set; }
        public string[] Messages { get; set; }
    }

    public class CommandResult<T> : CommandResult
    {
        public CommandResult(Exception e): base(e) {}
        public CommandResult() { }

        public T Result { get; set; }
    }

    public static class CommandResultTypes
    {
        public static string Update = "update";
        public static string Create = "insert";
        public static string None = "none";
        public static string Delete = "delete";
    }
}
