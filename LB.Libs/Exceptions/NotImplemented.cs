using System;
namespace LB.Libs
{
    public class NotImplemented : NotImplementedException
    {
        public NotImplemented(string methodName, object obj) : base($"Перезапишите метод {methodName}() У {obj.GetType().Name}!!!") { }
    }
}
