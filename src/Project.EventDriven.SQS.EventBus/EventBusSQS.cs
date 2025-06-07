using System.Text.Json;
using Amazon.SQS.Model;
using Autofac;
using Microsoft.Extensions.Logging;
using Project.EventDriven.SQS.EventBus.Connections;
using Project.EventDriven.SQS.EventBus.Events;
using Project.EventDriven.SQS.EventBus.Handlers;
using Project.EventDriven.SQS.EventBus.Managers;

namespace Project.EventDriven.SQS.EventBus;

public class EventBusSQS(
    ISQSConnection _connection,
    ISubscriptionManager _subscriptionManager,
    ILifetimeScope _lifetimeScope,
    ILogger<EventBusSQS> _logger
) : IEventBus
{
    public async Task Publish<T>(T @event, string topic = null) where T : Event
    {
        string eventName = RecoverEventName<T>(topic);
        
        _logger.LogInformation("Iniciando publicação no tópico {0}", eventName);

        try
        {
            await SendMessage(@event, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar no tópico {0}", eventName);

            _logger.LogError(ex, ex.Message + "\n" + ex.StackTrace);

            throw;
        }
        finally
        {
            _logger.LogInformation("Finalizando publicação no tópico {0}", eventName);
        }
    }

    public async Task Subscribe<T, TH>(CancellationToken stoppingToken, int maxConcurrentCalls = 10, string topic = null)
        where T : Event
        where TH : IEventHandler<T>
    {
        string eventName = RecoverEventName<T>(topic);

        _logger.LogInformation("Iniciando assinatura do tópico {0}", eventName);

        try
        {
            await Task.Run(async delegate
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ReceiveMessages<T, TH>(stoppingToken, maxConcurrentCalls, topic);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consumir o tópico {0}", eventName);

            _logger.LogError(ex, ex.Message + "\n" + ex.StackTrace);

            throw;
        }
        finally
        {
            _logger.LogInformation("Finalizando assinatura do tópico {0}", eventName);
        }
    }

    #region Private Methods
    private static string RecoverEventName<T>(string topic) where T : Event
    {
        if (!string.IsNullOrWhiteSpace(topic)) return topic;

        return typeof(T).Name.Replace("IntegrationEvent", "");
    }

    private async Task<string> SendMessage<T>(T @event, string topic) where T : Event
    {
        string queueUrl = GetQueueUrl<T>(topic);
        string message = GetMessage(@event);
        SendMessageResponse response = await _connection.Client.SendMessageAsync(queueUrl, message);
        _logger.LogInformation("Mensagem adicionada ao tópico: {0}", queueUrl);
        _logger.LogInformation("HttpStatusCode: {0}", response.HttpStatusCode);

        return response.MessageId;
    }

    private string GetMessage<T>(T @event) where T : Event
    {
        return JsonSerializer.Serialize((object)@event);
    }

    private string GetQueueUrl<T>(string topic = null) where T : Event
    {
        return _connection.UrlTopicBase + (string.IsNullOrWhiteSpace(topic) ? typeof(T).Name.Replace("IntegrationEvent", "") : topic);
    }

    private async Task ReceiveMessages<T, TH>(CancellationToken stoppingToken, int maxConcurrentCalls = 10, string topic = null)
    where T : Event
    where TH : IEventHandler<T>
    {
        if (stoppingToken.IsCancellationRequested) return;

        string queueUrl = GetQueueUrl<T>(topic);

        IEnumerable<Message> messages = await GetMessages<T, TH>(stoppingToken, maxConcurrentCalls, queueUrl);

        ParallelOptions options = new() { CancellationToken = stoppingToken, MaxDegreeOfParallelism = maxConcurrentCalls };

        await Parallel.ForEachAsync(
            parallelOptions: options,
            source: messages,
            body: async delegate (Message message, CancellationToken token)
            {
                string eventName = RecoverEventName<T>(topic);

                _logger.LogInformation("Iniciando consumo o tópico {0}", eventName);

                try
                {
                    IEnumerable<SubscriptionInfo> subscriptions = _subscriptionManager.GetHandlersForEvent<T>(topic);

                    foreach (SubscriptionInfo subscription in subscriptions)
                    {
                        T @event = GetEvent<T>(message);

                        IEventHandler<T> handler = GetHandler<T, TH>(subscription);

                        await handler.Handle(@event);

                        await DeleteMessage<T>(message, topic);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro `{ex.Message}` ocorreu durante o consumo do tópico {eventName}  mensagem {message.MessageId}");
                    _logger.LogError(ex, ex.Message + "\n" + ex.StackTrace);

                    throw;
                }
                finally
                {
                    _logger.LogInformation("Finalizando consumo o tópico {0} da mensagem {1}", eventName, message.MessageId);
                }
            });
    }

    private T GetEvent<T>(Message message) where T : Event
    {
        _ = int.TryParse(message.Attributes["ApproximateReceiveCount"], out int receiveCount);
        _ = Guid.TryParse(message.MessageId, out Guid messageId);

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

        T eventMessage = JsonSerializer.Deserialize<T>(message.Body, options);

        eventMessage.ReceiveCount = receiveCount;
        eventMessage.EventId = messageId;

        return eventMessage;
    }

    private IEventHandler<T> GetHandler<T, TH>(SubscriptionInfo subscription)
        where T : Event
        where TH : IEventHandler<T>
    {
        ILifetimeScope scope = _lifetimeScope.BeginLifetimeScope();
        return (IEventHandler<T>)scope.Resolve(subscription.HandlerType);
    }

    private async Task<IEnumerable<Message>> GetMessages<T, TH>(CancellationToken stoppingToken, int maxMessages, string queueUrl)
        where T : Event
        where TH : IEventHandler<T>
    {
        ReceiveMessageResponse response = await _connection.Client.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = maxMessages,
            WaitTimeSeconds = 20,
        }, stoppingToken);


        return response.Messages;
    }

    private async Task DeleteMessage<T>(Message message, string topic) where T : Event
    {
        string queueUrl = GetQueueUrl<T>(topic);

        await _connection.Client.DeleteMessageAsync(queueUrl, message.ReceiptHandle);

        _logger.LogInformation("Mensagem excluida ao tópico: {0}. ID: {1}", queueUrl, message.MessageId);
    }
    #endregion
}
