using System;
namespace DTO_PremierDucts.JWT_Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipAuthenticationHeadersAttribute : Attribute
    {
    }
}

