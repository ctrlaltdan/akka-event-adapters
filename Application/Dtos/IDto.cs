using System;

namespace Application.Dtos
{
    public interface IDto
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
        string HitType { get; }
    }
}
