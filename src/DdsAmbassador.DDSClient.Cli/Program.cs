using System.Text.Json;
using System.Text.Json.Serialization;
using DdsAmbassador.DDSClient;

var command = args.FirstOrDefault();
if (string.IsNullOrWhiteSpace(command) || IsHelp(command))
{
    PrintUsage();
    return command is null ? 1 : 0;
}

var options = CliOptions.Parse(args.Skip(1).ToArray());

try
{
    switch (command.ToLowerInvariant())
    {
        case "list":
            var clientOptions = DdsClientOptions.Load(options.ConfigPath);
            var configuration = DdsConfigurationLoader.Load(clientOptions);
            foreach (var topic in configuration.Topics.OrderBy(topic => topic.Name, StringComparer.Ordinal))
            {
                Console.WriteLine($"{topic.Name} direction={topic.Direction} qos={topic.QualifiedQosProfile}");
            }

            return 0;

        case "publish":
        case "send":
            RequireTopic(options.Topic);
            var publishTopic = options.Topic!;
            using (var client = DdsClient.Connect(options.ConfigPath))
            {
                var sampleType = ResolveMessageType(publishTopic);
                var sample = await CreateSample(sampleType, options);
                client.Publish(publishTopic, sample);
                Console.WriteLine($"Published {publishTopic} ({sampleType.FullName}).");
            }

            return 0;

        case "subscribe":
        case "listen":
            RequireTopic(options.Topic);
            var subscribeTopic = options.Topic!;
            using (var client = DdsClient.Connect(options.ConfigPath))
            using (var shutdown = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    shutdown.Cancel();
                };

                var sampleType = ResolveMessageType(subscribeTopic);
                using var subscription = client.Subscribe(subscribeTopic, sampleType, sample =>
                {
                    Console.WriteLine($"[{DateTimeOffset.Now:O}] {subscribeTopic}");
                    Console.WriteLine(sample);
                });

                Console.WriteLine($"Listening {subscribeTopic} on DDS domain {client.Options.DomainId}. Press Ctrl+C to stop.");
                await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token).ConfigureAwait(false);
            }

            return 0;

        default:
            Console.Error.WriteLine($"Unknown command: {command}");
            PrintUsage();
            return 1;
    }
}
catch (OperationCanceledException)
{
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static bool IsHelp(string value)
{
    return value.Equals("-h", StringComparison.OrdinalIgnoreCase)
        || value.Equals("--help", StringComparison.OrdinalIgnoreCase)
        || value.Equals("help", StringComparison.OrdinalIgnoreCase);
}

static void RequireTopic(string? topic)
{
    if (string.IsNullOrWhiteSpace(topic))
    {
        throw new ArgumentException("A message/topic name is required.");
    }
}

static Type ResolveMessageType(string topic)
{
    return typeof(DdsClient).Assembly.GetType($"MSG.{topic}", throwOnError: true)
        ?? throw new InvalidOperationException($"Could not resolve MSG.{topic}.");
}

static async Task<object> CreateSample(Type sampleType, CliOptions options)
{
    string? json = null;
    if (options.JsonPath is not null)
    {
        json = await File.ReadAllTextAsync(options.JsonPath).ConfigureAwait(false);
    }
    else if (options.ReadStdin)
    {
        json = await Console.In.ReadToEndAsync().ConfigureAwait(false);
    }

    object sample;
    if (!string.IsNullOrWhiteSpace(json))
    {
        sample = JsonSerializer.Deserialize(json, sampleType, CreateJsonOptions())
            ?? throw new InvalidOperationException($"Could not deserialize JSON as {sampleType.FullName}.");
    }
    else
    {
        sample = Activator.CreateInstance(sampleType)
            ?? throw new InvalidOperationException($"Could not create {sampleType.FullName}.");
    }

    ApplyCommonDefaults(sample, options.Topic);
    return sample;
}

static void ApplyCommonDefaults(object sample, string? topic)
{
    var headerProperty = sample.GetType().GetProperty("Header");
    if (headerProperty?.CanRead != true || headerProperty.CanWrite != true)
    {
        return;
    }

    var header = headerProperty.GetValue(sample);
    if (header is null)
    {
        header = Activator.CreateInstance(headerProperty.PropertyType);
        headerProperty.SetValue(sample, header);
    }

    if (header is null)
    {
        return;
    }

    SetIfDefault(header, "TimeStamp", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    SetEnumIfDefault(header, "MsgID", topic);
    SetEnumIfDefault(header, "SrcSimID", Environment.GetEnvironmentVariable("DDS_CLI_SRC_SIM_ID") ?? "TCC");
    SetEnumIfDefault(header, "DstSimID", Environment.GetEnvironmentVariable("DDS_CLI_DST_SIM_ID") ?? "TDS");
}

static void SetIfDefault<T>(object target, string propertyName, T value)
{
    var property = target.GetType().GetProperty(propertyName);
    if (property?.CanRead != true || property.CanWrite != true || property.PropertyType != typeof(T))
    {
        return;
    }

    if (EqualityComparer<T>.Default.Equals((T?)property.GetValue(target), default))
    {
        property.SetValue(target, value);
    }
}

static void SetEnumIfDefault(object target, string propertyName, string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return;
    }

    var property = target.GetType().GetProperty(propertyName);
    if (property?.CanRead != true || property.CanWrite != true || !property.PropertyType.IsEnum)
    {
        return;
    }

    var current = property.GetValue(target);
    if (!Equals(current, Activator.CreateInstance(property.PropertyType)))
    {
        return;
    }

    if (Enum.TryParse(property.PropertyType, value, ignoreCase: true, out var parsed))
    {
        property.SetValue(target, parsed);
    }
}

static void PrintUsage()
{
    Console.WriteLine(
        """
        DDS Client CLI

        Usage:
          ddsclient list [--config <path>]
          ddsclient publish <MessageName> [--config <path>] [--json <path>|--stdin]
          ddsclient subscribe <MessageName> [--config <path>]

        Examples:
          ddsclient list --config /app/definitions/dds_client.asap.xml
          ddsclient publish TimeTickInformation --config /app/definitions/dds_client.asap.xml
          ddsclient publish SetSimulation --json /tmp/set-simulation.json
          ddsclient subscribe TimeTickInformation --config /app/definitions/dds_client.asap.xml

        Debug logging:
          Set <log_level>Debug</log_level> in dds_client.xml, or set DDS_LOG_LEVEL=Debug.
        """);
}

static JsonSerializerOptions CreateJsonOptions()
{
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
}

internal sealed class CliOptions
{
    public string? ConfigPath { get; private init; }

    public string? Topic { get; private init; }

    public string? JsonPath { get; private init; }

    public bool ReadStdin { get; private init; }

    public static CliOptions Parse(string[] args)
    {
        string? configPath = null;
        string? topic = null;
        string? jsonPath = null;
        var readStdin = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--config":
                case "-c":
                    configPath = ReadValue(args, ref i);
                    break;
                case "--json":
                case "-j":
                    jsonPath = ReadValue(args, ref i);
                    break;
                case "--stdin":
                    readStdin = true;
                    break;
                default:
                    topic ??= args[i];
                    break;
            }
        }

        return new CliOptions
        {
            ConfigPath = configPath,
            Topic = topic,
            JsonPath = jsonPath,
            ReadStdin = readStdin
        };
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{args[index]} requires a value.");
        }

        index++;
        return args[index];
    }
}
