using CurrencyConverter.Application.Abstractions;

namespace CurrencyConverter.WebApi.Common;

internal sealed class DefaultContextAccessor
{
    private static readonly AsyncLocal<ContextHolder> Holder = new();

    public IRequestContext? Context
    {
        get => Holder.Value?.Context;
        set
        {
            var holder = Holder.Value;
            if (holder != null)
            {
                holder.Context = null;
            }

            if (value != null)
            {
                Holder.Value = new ContextHolder { Context = value };
            }
        }
    }

    private class ContextHolder
    {
        public IRequestContext? Context;
    }
}