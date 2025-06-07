
namespace Project.EventDriven.SQS.EventBus.Managers;

/// <summary>
/// Contém informações sobre um handler inscrito para um tipo específico de evento.
/// </summary>
/// <param name="handlerType">Tipo do handler (implementa IEventHandler&lt;T&gt;).</param>
public class SubscriptionInfo(Type handlerType)
{
    /// <summary>
    /// Tipo do handler que será invocado quando o evento for processado.
    /// </summary>
    public Type HandlerType { get; } = handlerType;
}
