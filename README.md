# 📬 EventBus com AWS SQS e InMemory Subscription Manager (.NET)

Este projeto implementa um barramento de eventos assíncrono e extensível com suporte a:

* Publicação e assinatura de eventos (`IEventBus`);
* Integração com AWS SQS;
* Gerenciamento de handlers com escopo controlado via Autofac (`ILifetimeScope`);
* Registro de handlers em memória com suporte a múltiplos tópicos.

---

## 🤩 Estrutura de Componentes

### `Event`

Classe base para todos os eventos enviados no barramento.

```csharp
public class Event : IDisposable
{
    public Guid EventId { get; set; }
    public DateTime CreationDate { get; set; }
    public int ReceiveCount { get; set; }
}
```

---

### `IEventHandler<TEvent>`

Interface para implementar handlers de eventos.

```csharp
public interface IEventHandler<in TEvent> where TEvent : Event
{
    Task Handle(TEvent @event);
}
```

---

### `IEventBus`

Interface principal para publicação e assinatura de eventos.

```csharp
public interface IEventBus
{
    Task Publish<T>(T @event, string topico = null) where T : Event;

    Task Subscribe<T, TH>(
        CancellationToken stoppingToken,
        int maxConcurrentCalls = 10,
        string topico = null)
        where T : Event
        where TH : IEventHandler<T>;
}
```

---

### `ISubscriptionManager` e `SubscriptionInfo`

Gerencia o registro de handlers para eventos, opcionalmente por tópico.

```csharp
public interface ISubscriptionManager
{
    void AddSubscription<T, TH>(string topic = null);
    void RemoveSubscription<T, TH>(string topic = null);
    bool HasSubscriptionForEvent<T>(string topic = null);
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>(string topic = null);
}
```

---

### `SQSSettings` e `ISQSConnection`

Fornecem os dados e cliente necessários para integrar com AWS SQS.

```csharp
public class SQSSettings
{
    public string Region { get; set; }
    public string Account { get; set; }
}

public interface ISQSConnection
{
    SQSSettings Settings { get; }
    IAmazonSQS Client { get; }
    string UrlTopicBase { get; }
}
```

---

## ⚙️ Injeção de Dependência (Autofac)

```csharp
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterType<InMemoryEventBusWithAutofac>()
            .As<IEventBus>()
            .SingleInstance();

    container.RegisterType<InMemoryEventBusSubscriptionManager>()
            .As<ISubscriptionManager>()
            .SingleInstance();

    container.RegisterType<SendWelcomeEmailHandler>()
            .As<IEventHandler<UserCreatedEvent>>();
});
```

---

## 🧪 Exemplo de Uso

### Publicando um Evento

```csharp
await _eventBus.Publish(new UserCreatedEvent { UserId = "123" });
```

### Assinando um Evento

```csharp
await _eventBus.Subscribe<UserCreatedEvent, SendWelcomeEmailHandler>(stoppingToken);
```

---

## 🚀 Extensões Futuras

* ✅ Suporte completo à AWS SQS (envio e consumo);
* ✅ Suporte a múltiplos tópicos;
* ⏳ Persistência de eventos (Event Sourcing);
* ⏳ Retentativas com política de backoff.

---


---

## 📜 Licença

Este projeto é fornecido como exemplo e pode ser adaptado livremente para uso em produção ou estudos internos.
