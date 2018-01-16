using System;

namespace Core
{
    public interface ICommand
    {
        Guid Id { get; set; }
    }
}
