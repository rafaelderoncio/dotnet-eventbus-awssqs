namespace Project.EventDriven.SQS.EventBus.Settings;

/// <summary>
/// Representa as configurações necessárias para conectar ao serviço Amazon SQS.
/// </summary>
public class SQSSettings
{
    /// <summary>
    /// Região da AWS onde a fila SQS está localizada (ex: "us-east-1").
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// ID da conta da AWS associada à fila SQS.
    /// </summary>
    public string Account { get; set; } = string.Empty;
}
