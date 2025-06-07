using System.Text.Json.Serialization;

namespace Project.EventDriven.SQS.EventBus.Events;

/// <summary>
/// Representa um evento base utilizado no sistema de mensageria.
/// Inclui metadados comuns como ID, data de criação e contador de recebimentos.
/// Implementa IDisposable para cenários que exijam liberação explícita de recursos.
/// </summary>
public class Event : IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// Identificador único do evento.
    /// Gerado automaticamente no momento da criação da instância.
    /// Ignorado na serialização JSON.
    /// </summary>
    [JsonIgnore]
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Data e hora (UTC) em que o evento foi criado.
    /// Usado para rastreamento e controle de tempo de vida.
    /// Ignorado na serialização JSON.
    /// </summary>
    [JsonIgnore]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Número de vezes que o evento foi recebido/processado.
    /// Útil para controle de reentregas em sistemas de fila como SQS.
    /// Ignorado na serialização JSON.
    /// </summary>
    [JsonIgnore]
    public int ReceiveCount { get; set; } = 0;

    /// <summary>
    /// Libera os recursos utilizados pelo evento.
    /// </summary>
    /// <param name="disposing">Indica se a liberação está sendo feita explicitamente.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Liberar recursos gerenciados, se necessário.
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// Método para liberar os recursos do evento.
    /// Pode ser chamado manualmente ou via using.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

