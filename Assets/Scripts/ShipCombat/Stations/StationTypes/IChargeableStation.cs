using System;

public interface IChargeableStation
{
    // Any station using this must provide this exact event
    event Action<float, float> OnChargeChanged;
}