using System.Collections;

namespace Core
{
    public interface IHandleCommand<TCommand>
    {
        IEnumerable Handle(TCommand command);
    }
}
