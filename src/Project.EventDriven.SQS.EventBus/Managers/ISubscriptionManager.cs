using Project.EventDriven.SQS.EventBus.Events;
using Project.EventDriven.SQS.EventBus.Handlers;

namespace Project.EventDriven.SQS.EventBus.Managers;

/// <summary>
/// Interface responsável por gerenciar as inscrições (subscriptions) de eventos e seus respectivos handlers.
/// Permite registrar, remover, limpar e consultar handlers associados a eventos.
/// </summary>
public interface ISubscriptionManager
{
    /// <summary>
    /// Indica se não há nenhuma inscrição registrada.
    /// </summary>
    /// <returns>Verdadeiro se não houver inscrições; caso contrário, falso.</returns>
    bool IsEmpty();

    /// <summary>
    /// Remove todas as inscrições registradas.
    /// </summary>
    void Clear();

    /// <summary>
    /// Adiciona uma nova inscrição para um evento específico.
    /// </summary>
    /// <typeparam name="T">Tipo do evento.</typeparam>
    /// <typeparam name="TH">Tipo do handler do evento.</typeparam>
    /// <param name="topic">Tópico ou canal opcional associado ao evento.</param>
    void AddSubscription<T, TH>(string topic = null)
        where T : Event
        where TH : IEventHandler<T>;

    /// <summary>
    /// Remove uma inscrição existente para um evento e handler específicos.
    /// </summary>
    /// <typeparam name="T">Tipo do evento.</typeparam>
    /// <typeparam name="TH">Tipo do handler do evento.</typeparam>
    /// <param name="topic">Tópico opcional.</param>
    void RemoveSubscription<T, TH>(string topic = null)
        where T : Event
        where TH : IEventHandler<T>;

    /// <summary>
    /// Verifica se existe ao menos um handler inscrito para o tipo de evento especificado.
    /// </summary>
    /// <typeparam name="T">Tipo do evento.</typeparam>
    /// <param name="topic">Tópico opcional.</param>
    /// <returns>Verdadeiro se houver algum handler registrado; caso contrário, falso.</returns>
    bool HasSubscriptionForEvent<T>(string topic = null) where T : Event;

    /// <summary>
    /// Obtém todos os handlers registrados para um tipo de evento específico.
    /// </summary>
    /// <typeparam name="T">Tipo do evento.</typeparam>
    /// <param name="topic">Tópico opcional.</param>
    /// <returns>Uma lista de informações dos handlers registrados.</returns>
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>(string topic = null) where T : Event;
}
