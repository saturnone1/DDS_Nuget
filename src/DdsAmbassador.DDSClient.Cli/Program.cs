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
            PrintTopics(configuration);

            return 0;

        case "shell":
        case "daemon":
            return await RunShell(options).ConfigureAwait(false);

        case "idle":
        case "wait":
            using (var client = DdsClient.Connect(options.ConfigPath))
            using (var shutdown = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    shutdown.Cancel();
                };

                Console.WriteLine(
                    $"DDS participant가 초기화되었습니다. domain={client.Options.DomainId}. 중지하려면 Ctrl+C를 누르세요.");
                await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token).ConfigureAwait(false);
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
                for (var index = 0; index < options.Repeat; index++)
                {
                    client.Publish(publishTopic, sample);
                    if (index + 1 < options.Repeat && options.IntervalMs > 0)
                    {
                        await Task.Delay(options.IntervalMs).ConfigureAwait(false);
                    }
                }
                Console.WriteLine($"송신 완료: {publishTopic} ({sampleType.FullName}), count={options.Repeat}");
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
                var receivedCount = 0;
                var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                using var subscription = client.Subscribe(subscribeTopic, sampleType, sample =>
                {
                    Console.WriteLine($"[{DateTimeOffset.Now:O}] {subscribeTopic}");
                    Console.WriteLine(sample);
                    if (options.Count > 0 && Interlocked.Increment(ref receivedCount) >= options.Count)
                    {
                        completed.TrySetResult();
                    }
                });

                Console.WriteLine($"수신 대기 중: {subscribeTopic}, DDS domain={client.Options.DomainId}. 중지하려면 Ctrl+C를 누르세요.");
                if (options.Count > 0)
                {
                    if (options.TimeoutMs > 0)
                    {
                        await completed.Task.WaitAsync(TimeSpan.FromMilliseconds(options.TimeoutMs), shutdown.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        await completed.Task.WaitAsync(shutdown.Token).ConfigureAwait(false);
                    }
                }
                else if (options.TimeoutMs > 0)
                {
                    await Task.Delay(options.TimeoutMs, shutdown.Token).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token).ConfigureAwait(false);
                }
            }

            return 0;

        default:
            Console.Error.WriteLine($"알 수 없는 명령입니다: {command}");
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
        throw new ArgumentException("메시지/topic 이름이 필요합니다.");
    }
}

static Type ResolveMessageType(string topic)
{
    return typeof(DdsClient).Assembly.GetType($"MSG.{topic}", throwOnError: true)
        ?? throw new InvalidOperationException($"MSG.{topic} 타입을 찾을 수 없습니다.");
}

static async Task<int> RunShell(CliOptions options)
{
    using var client = DdsClient.Connect(options.ConfigPath);
    using var shutdown = new CancellationTokenSource();
    var subscriptions = new Dictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);

    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        shutdown.Cancel();
    };

    PrintShellWelcome(client);

    try
    {
        while (!shutdown.IsCancellationRequested)
        {
            Console.Write("ddsclient(도움말)> ");
            var line = await ReadShellLine(shutdown.Token).ConfigureAwait(false);
            if (line is null)
            {
                continue;
            }

            var tokens = SplitCommandLine(line);
            if (tokens.Count == 0)
            {
                PrintShellWelcome(client);
                continue;
            }

            try
            {
                var command = tokens[0].ToLowerInvariant();
                switch (command)
                {
                    case "help":
                    case "?":
                        PrintShellWelcome(client);
                        break;

                    case "exit":
                    case "quit":
                        shutdown.Cancel();
                        break;

                    case "list":
                        PrintTopics(client.Configuration);
                        break;

                    case "status":
                        WriteInfo($"domain={client.Options.DomainId}, 수신 중인 topic={subscriptions.Count}");
                        foreach (var topic in subscriptions.Keys.OrderBy(topic => topic, StringComparer.Ordinal))
                        {
                            Console.WriteLine($"  수신 중: {topic}");
                        }
                        break;

                    case "publish":
                    case "send":
                        var publishOptions = CliOptions.Parse(tokens.Skip(1).ToArray());
                        RequireTopic(publishOptions.Topic);
                        if (publishOptions.ReadStdin)
                        {
                            throw new ArgumentException("shell 모드에서는 --stdin을 지원하지 않습니다. --json <path>를 쓰거나 payload를 생략하세요.");
                        }

                        var publishTopic = publishOptions.Topic!;
                        var publishType = ResolveMessageType(publishTopic);
                        var sample = await CreateSample(publishType, publishOptions).ConfigureAwait(false);
                        for (var index = 0; index < publishOptions.Repeat; index++)
                        {
                            client.Publish(publishTopic, sample);
                            if (index + 1 < publishOptions.Repeat && publishOptions.IntervalMs > 0)
                            {
                                await Task.Delay(publishOptions.IntervalMs, shutdown.Token).ConfigureAwait(false);
                            }
                        }
                        WriteSuccess($"송신 완료: {publishTopic} ({publishType.FullName}), count={publishOptions.Repeat}");
                        break;

                    case "subscribe":
                    case "listen":
                        var subscribeOptions = CliOptions.Parse(tokens.Skip(1).ToArray());
                        RequireTopic(subscribeOptions.Topic);
                        var subscribeTopic = subscribeOptions.Topic!;
                        if (subscriptions.ContainsKey(subscribeTopic))
                        {
                            WriteWarning($"이미 수신 중입니다: {subscribeTopic}");
                            break;
                        }

                        var subscribeType = ResolveMessageType(subscribeTopic);
                        subscriptions[subscribeTopic] = client.Subscribe(subscribeTopic, subscribeType, received =>
                        {
                            Console.WriteLine();
                            Console.WriteLine($"[{DateTimeOffset.Now:O}] {subscribeTopic}");
                            Console.WriteLine(received);
                            Console.Write("ddsclient(도움말)> ");
                        });
                        WriteSuccess($"수신 시작: {subscribeTopic} ({subscribeType.FullName})");
                        break;

                    case "unsubscribe":
                    case "unsub":
                        var target = tokens.ElementAtOrDefault(1);
                        if (string.IsNullOrWhiteSpace(target))
                        {
                            throw new ArgumentException("unsubscribe에는 topic 이름 또는 all이 필요합니다.");
                        }

                        if (target.Equals("all", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var subscription in subscriptions.Values)
                            {
                                subscription.Dispose();
                            }

                            subscriptions.Clear();
                            WriteSuccess("모든 topic 수신을 해제했습니다.");
                        }
                        else if (subscriptions.Remove(target, out var subscription))
                        {
                            subscription.Dispose();
                            WriteSuccess($"수신 해제: {target}");
                        }
                        else
                        {
                            WriteWarning($"수신 중인 topic이 아닙니다: {target}");
                        }

                        break;

                    default:
                        WriteWarning($"알 수 없는 shell 명령입니다: {tokens[0]}");
                        PrintShellHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteError(ex.Message);
            }
        }
    }
    finally
    {
        foreach (var subscription in subscriptions.Values)
        {
            subscription.Dispose();
        }
    }

    return 0;
}

static async Task<string?> ReadShellLine(CancellationToken cancellationToken)
{
    try
    {
        var line = await Console.In.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        if (line is not null)
        {
            return line;
        }
    }
    catch (OperationCanceledException)
    {
        throw;
    }

    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
    return null;
}

static void PrintTopics(DdsConfiguration configuration)
{
    foreach (var topic in configuration.Topics.OrderBy(topic => topic.Name, StringComparer.Ordinal))
    {
        Console.WriteLine($"{topic.Name} direction={topic.Direction} qos={topic.QualifiedQosProfile}");
    }
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
            ?? throw new InvalidOperationException($"JSON을 {sampleType.FullName} 타입으로 변환할 수 없습니다.");
    }
    else
    {
        sample = Activator.CreateInstance(sampleType)
            ?? throw new InvalidOperationException($"{sampleType.FullName} 인스턴스를 생성할 수 없습니다.");
    }

    ApplyCommonDefaults(sample, options.Topic, json);
    return sample;
}

static void ApplyCommonDefaults(object sample, string? topic, string? json)
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
    SetEnumIfMissingOrDefault(header, "MsgID", topic, json);
    SetEnumIfMissingOrDefault(
        header, "SrcSimID", Environment.GetEnvironmentVariable("DDS_CLI_SRC_SIM_ID") ?? "TCC", json);
    SetEnumIfMissingOrDefault(
        header, "DstSimID", Environment.GetEnvironmentVariable("DDS_CLI_DST_SIM_ID") ?? "TDS", json);
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

static void SetEnumIfMissingOrDefault(object target, string propertyName, string? value, string? json)
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
    if (ContainsJsonProperty(json, propertyName) &&
        !Equals(current, Activator.CreateInstance(property.PropertyType)))
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

        사용법:
          ddsclient list [--config <path>]
          ddsclient shell [--config <path>]
          ddsclient idle [--config <path>]
          ddsclient publish <MessageName> [--config <path>] [--json <path>|--stdin] [--repeat <n>] [--interval-ms <n>]
          ddsclient subscribe <MessageName> [--config <path>] [--count <n>] [--timeout-ms <n>]

        예시:
          ddsclient list --config /app/definitions/dds_client.asap.xml
          ddsclient shell --config /app/definitions/dds_client.asap.xml
          ddsclient idle --config /app/definitions/dds_client.asap.xml
          ddsclient publish TimeTickInformation --config /app/definitions/dds_client.asap.xml
          ddsclient publish SetSimulation --json /tmp/set-simulation.json
          ddsclient subscribe TimeTickInformation --config /app/definitions/dds_client.asap.xml

        디버그 로그:
          dds_client.xml에서 <log_level>Debug</log_level>로 설정하거나 DDS_LOG_LEVEL=Debug 환경변수를 설정하세요.
        """);
}

static void PrintShellWelcome(DdsClient client)
{
    WriteLineColor("DDS Client Shell", ConsoleColor.Black, ConsoleColor.Cyan);
    WriteSuccess($"DDS participant가 초기화되었고 계속 유지됩니다. domain={client.Options.DomainId}");
    Console.WriteLine();
    Console.WriteLine("이 shell은 하나의 DdsClient/DomainParticipant를 계속 유지합니다.");
    Console.WriteLine("Discovery UI에 보이는 participant와 여기서 송수신하는 participant는 같습니다.");
    Console.WriteLine();
    WriteInfo("Kubernetes에서 다시 붙는 쉬운 명령:");
    Console.WriteLine("  kubectl -n dds-test attach -it deploy/ddsclient-cli");
    Console.WriteLine();
    WriteWarning("나갈 때 exit를 입력하면 participant가 종료되고 Pod가 재시작될 수 있습니다.");
    Console.WriteLine("participant를 유지하고 빠져나오려면 Ctrl+P 다음 Ctrl+Q를 누르세요.");
    Console.WriteLine();
    PrintShellHelp();
}

static void PrintShellHelp()
{
    Console.WriteLine("이 shell은 하나의 DdsClient/DomainParticipant를 계속 유지합니다.");
    Console.WriteLine("Discovery UI에 보이는 participant와 여기서 송수신하는 participant는 같습니다.");
    Console.WriteLine();
    WriteInfo("Kubernetes에서 다시 붙는 쉬운 명령:");
    Console.WriteLine("  kubectl -n dds-test attach -it deploy/ddsclient-cli");
    Console.WriteLine();
    WriteWarning("나갈 때 exit를 입력하면 participant가 종료되고 Pod가 재시작될 수 있습니다.");
    Console.WriteLine("participant를 유지하고 빠져나오려면 Ctrl+P 다음 Ctrl+Q를 누르세요.");
    Console.WriteLine();
    WriteInfo("사용 가능한 명령:");
    Console.WriteLine("  list                                  topic 목록");
    Console.WriteLine("  status                                domain/subscription 상태");
    Console.WriteLine("  subscribe TimeTickInformation         메시지 수신 시작");
    Console.WriteLine("  publish TimeTickInformation           기본 메시지 송신");
    Console.WriteLine("  publish SetSimulation --json /tmp/a.json");
    Console.WriteLine("  unsubscribe TimeTickInformation       수신 해제");
    Console.WriteLine("  unsubscribe all                       모든 수신 해제");
    Console.WriteLine("  help                                  도움말 다시 보기");
    Console.WriteLine("  exit                                  shell 종료");
    Console.WriteLine();
}

static void WriteSuccess(string message)
{
    WriteLineColor($"[성공] {message}", ConsoleColor.Green);
}

static void WriteInfo(string message)
{
    WriteLineColor($"[정보] {message}", ConsoleColor.Cyan);
}

static void WriteWarning(string message)
{
    WriteLineColor($"[주의] {message}", ConsoleColor.Yellow);
}

static void WriteError(string message)
{
    WriteLineColor($"[오류] {message}", ConsoleColor.Red);
}

static void WriteLineColor(string message, ConsoleColor foreground, ConsoleColor? background = null)
{
    var originalForeground = Console.ForegroundColor;
    var originalBackground = Console.BackgroundColor;

    Console.ForegroundColor = foreground;
    if (background.HasValue)
    {
        Console.BackgroundColor = background.Value;
    }

    Console.WriteLine(message);
    Console.ForegroundColor = originalForeground;
    Console.BackgroundColor = originalBackground;
}

static IReadOnlyList<string> SplitCommandLine(string line)
{
    var tokens = new List<string>();
    var current = new System.Text.StringBuilder();
    var inQuotes = false;

    foreach (var c in line)
    {
        if (c == '"')
        {
            inQuotes = !inQuotes;
            continue;
        }

        if (char.IsWhiteSpace(c) && !inQuotes)
        {
            AddCurrentToken(tokens, current);
            continue;
        }

        current.Append(c);
    }

    AddCurrentToken(tokens, current);
    return tokens;
}

static void AddCurrentToken(ICollection<string> tokens, System.Text.StringBuilder current)
{
    if (current.Length == 0)
    {
        return;
    }

    tokens.Add(current.ToString());
    current.Clear();
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

static bool ContainsJsonProperty(string? json, string propertyName)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        return false;
    }

    using var document = JsonDocument.Parse(json);
    return ContainsJsonPropertyElement(document.RootElement, propertyName);
}

static bool ContainsJsonPropertyElement(JsonElement element, string propertyName)
{
    if (element.ValueKind == JsonValueKind.Object)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase) ||
                ContainsJsonPropertyElement(property.Value, propertyName))
            {
                return true;
            }
        }
    }
    else if (element.ValueKind == JsonValueKind.Array)
    {
        foreach (var item in element.EnumerateArray())
        {
            if (ContainsJsonPropertyElement(item, propertyName))
            {
                return true;
            }
        }
    }

    return false;
}

internal sealed class CliOptions
{
    public string? ConfigPath { get; private init; }

    public string? Topic { get; private init; }

    public string? JsonPath { get; private init; }

    public bool ReadStdin { get; private init; }

    public int Count { get; private init; }

    public int TimeoutMs { get; private init; }

    public int Repeat { get; private init; } = 1;

    public int IntervalMs { get; private init; }

    public static CliOptions Parse(string[] args)
    {
        string? configPath = null;
        string? topic = null;
        string? jsonPath = null;
        var readStdin = false;
        var count = 0;
        var timeoutMs = 0;
        var repeat = 1;
        var intervalMs = 0;

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
                case "--count":
                    count = ReadNonNegativeInt(args, ref i);
                    break;
                case "--timeout-ms":
                    timeoutMs = ReadNonNegativeInt(args, ref i);
                    break;
                case "--repeat":
                    repeat = ReadPositiveInt(args, ref i);
                    break;
                case "--interval-ms":
                    intervalMs = ReadNonNegativeInt(args, ref i);
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
            ReadStdin = readStdin,
            Count = count,
            TimeoutMs = timeoutMs,
            Repeat = repeat,
            IntervalMs = intervalMs
        };
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{args[index]} 옵션에는 값이 필요합니다.");
        }

        index++;
        return args[index];
    }

    private static int ReadNonNegativeInt(string[] args, ref int index)
    {
        var option = args[index];
        var value = ReadValue(args, ref index);
        return int.TryParse(value, out var parsed) && parsed >= 0
            ? parsed
            : throw new ArgumentException($"{option} 옵션에는 0 이상의 정수가 필요합니다.");
    }

    private static int ReadPositiveInt(string[] args, ref int index)
    {
        var option = args[index];
        var value = ReadValue(args, ref index);
        return int.TryParse(value, out var parsed) && parsed > 0
            ? parsed
            : throw new ArgumentException($"{option} 옵션에는 1 이상의 정수가 필요합니다.");
    }
}
