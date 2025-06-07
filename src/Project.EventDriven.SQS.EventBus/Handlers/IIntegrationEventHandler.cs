using Project.EventDriven.SQS.EventBus.Events;

namespace Project.EventDriven.SQS.EventBus.Handlers;

/// <summary>
/// Interface para manipulação de eventos de integração.
/// Implementações dessa interface devem conter a lógica necessária para processar o evento especificado.
/// </summary>
/// <typeparam name="TIntegrationEvent">
/// Tipo do evento que será tratado, derivado da classe <see cref="Event"/>.
/// </typeparam>
public interface IEventHandler<in TIntegrationEvent> where TIntegrationEvent : Event
{
    /// <summary>
    /// Manipula a lógica de processamento do evento recebido.
    /// </summary>
    /// <param name="event">Instância do evento a ser processado.</param>
    /// <returns>Uma tarefa que representa a conclusão da operação de tratamento.</returns>
    Task Handle(TIntegrationEvent @event);
}
