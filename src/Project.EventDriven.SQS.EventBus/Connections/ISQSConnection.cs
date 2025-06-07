using Amazon.SQS;
using Project.EventDriven.SQS.EventBus.Settings;

namespace Project.EventDriven.SQS.EventBus.Connections;

/// <summary>
/// Representa uma conexão com o serviço Amazon SQS,
/// fornecendo acesso às configurações, cliente SQS e base da URL das filas.
/// </summary>
public interface ISQSConnection
{
    /// <summary>
    /// Configurações de conexão com a SQS, incluindo região e ID da conta.
    /// </summary>
    SQSSettings Settings { get; }

    /// <summary>
    /// Cliente SQS da AWS para envio e recebimento de mensagens.
    /// </summary>
    IAmazonSQS Client { get; }

    /// <summary>
    /// Base da URL das filas SQS com base na região e na conta.
    /// Exemplo: https://sqs.us-east-1.amazonaws.com/123456789012/
    /// </summary>
    string UrlTopicBase => $"https://sqs.{Settings.Region}.amazonaws.com/{Settings.Account}/";
}

