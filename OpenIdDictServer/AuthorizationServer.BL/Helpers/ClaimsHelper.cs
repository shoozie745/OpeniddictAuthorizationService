using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace AuthorizationServer.BL.Helpers;

public class ClaimsHelper
{
    public static T GetValue<T>(ClaimsIdentity identity, string claimName)
    {
        Claim first = identity.FindFirst((Predicate<Claim>) (x => x.Type == claimName));
        if (first == null)
            return default (T);
        if (string.IsNullOrWhiteSpace(first.Value))
            return default (T);
        
        try
        {
            return (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromInvariantString(first.Value);
        }
        catch (Exception ex)
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
            interpolatedStringHandler.AppendFormatted(first.Value);
            interpolatedStringHandler.AppendLiteral(" from ");
            interpolatedStringHandler.AppendFormatted(first.Value);
            interpolatedStringHandler.AppendLiteral(" to ");
            interpolatedStringHandler.AppendFormatted<Type>(typeof (T));
            throw new InvalidCastException(interpolatedStringHandler.ToStringAndClear(), ex);
        }
    }
}