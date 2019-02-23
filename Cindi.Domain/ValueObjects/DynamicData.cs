using Cindi.Domain.Exceptions;
using Cindi.Domain.Exceptions.Global;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace Cindi.Domain.ValueObjects
{
    public class DynamicData
    {
        //public enum InputDataType { Int, String, Bool, Object, ErrorMessage, Decimal, DateTime }

        public DynamicData()
        {  }

        public DynamicData(string id, int type, string value)
        {
            this.Id = id;
            //this.Type = type;

            try
            {/*
                switch (type)
                {
                    case (int)InputDataType.Bool:
                        var result = Convert.ToBoolean(value);
                        break;
                    case (int)InputDataType.String:
                        break;
                    case (int)InputDataType.Int:
                        var intConversion = int.Parse(value);
                        break;
                    case (int)InputDataType.Object:
                        var jsonConversion = JsonConvert.DeserializeObject(value);
                        break;
                    case (int)InputDataType.Decimal:
                        var decimalConversion = decimal.Parse(value);
                        break;
                    case (int)InputDataType.DateTime:
                        var dateConversion = DateTime.Parse(value);
                        break;
                }*/
                Value = value;
            }
            catch (Exception e)
            {
                throw new InvalidInputValueException(e.Message);
            }
        }

        public string Id { get; set; }
        public int Type { get; set; }
        public object Value { get; set; }
    }
}
