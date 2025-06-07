using Project.EventDriven.SQS.EventBus.Events;
using Project.EventDriven.SQS.EventBus.Handlers;

namespace Project.EventDriven.SQS.EventBus;

/// <summary>
/// Interface que define um barramento de eventos assíncrono.
/// Permite publicar eventos e registrar handlers para processá-los.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica um evento no barramento, opcionalmente especificando um tópico.
    /// </summary>
    /// <typeparam name="T">Tipo do evento, derivado de <see cref="Event"/>.</typeparam>
    /// <param name="event">Instância do evento a ser publicado.</param>
    /// <param name="topico">Nome opcional do tópico (ex: nome da fila ou canal).</param>
    /// <returns>Uma tarefa assíncrona que representa a operação de publicação.</returns>
    Task Publish<T>(T @event, string topico = null) where T : Event;

    /// <summary>
    /// Registra a assinatura de um handler para um determinado tipo de evento.
    /// Inicia o consumo das mensagens de forma assíncrona.
    /// </summary>
    /// <typeparam name="T">Tipo do evento a ser manipulado.</typeparam>
    /// <typeparam name="TH">Tipo do handler que processa o evento.</typeparam>
    /// <param name="stoppingToken">Token de cancelamento para encerrar o consumo de forma graciosa.</param>
    /// <param name="maxConcurrentCalls">Número máximo de mensagens processadas em paralelo.</param>
    /// <param name="topico">Nome opcional do tópico associado ao evento.</param>
    /// <returns>Uma tarefa assíncrona que representa a operação de inscrição.</returns>
    Task Subscribe<T, TH>(CancellationToken stoppingToken, int maxConcurrentCalls = 10, string topico = null)
        where T : Event
        where TH : IEventHandler<T>;
}
