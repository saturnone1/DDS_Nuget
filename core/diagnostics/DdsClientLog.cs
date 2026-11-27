namespace DdsAmbassador.DDSClient;

/// <summary>진단 출력 실패를 통신 오류와 분리하는 최선형 로깅 도우미다.</summary>
/// <remarks>콘솔 스트림이나 생성 타입의 문자열 변환이 실패해도 발행·수신을 중단하지 않는다.
/// 로그는 오류 관찰자를 대신하지 않으며, 로그 수준 확인은 가능하면 문자열 생성 전에 수행한다.</remarks>
internal static class DdsClientLog
{
    /// <summary>로그 문자열을 만들기 전에 수준을 확인한다. 인수 문자열은 호출 전에 평가되므로 로깅이 꺼져 있어도 메시지 직렬화 비용이 발생할 수 있다.</summary>
    public static bool IsEnabled(DdsClientOptions options, DdsLogLevel level)
    {
        return options.LogLevel >= level;
    }

    public static void Error(DdsClientOptions options, string message, Exception? exception = null)
    {
        if (options.LogLevel < DdsLogLevel.Error)
        {
            return;
        }

        try
        {
            Console.Error.WriteLine($"[{DateTimeOffset.Now:O}] [DDS] [ERROR] {message}");
            if (exception is not null) Console.Error.WriteLine(exception);
        }
        catch (Exception) { /* 로그 실패가 통신이나 오류 통지를 중단하면 안 된다. */ }
    }

    public static void Debug(DdsClientOptions options, string message)
    {
        if (options.LogLevel < DdsLogLevel.Debug)
        {
            return;
        }

        try { Console.WriteLine($"[{DateTimeOffset.Now:O}] [DDS] [DEBUG] {message}"); }
        catch (Exception) { /* 출력 스트림 실패는 통신과 분리한다. */ }
    }

    public static void Sample(DdsClientOptions options, string direction, TopicDefinition topic, object sample)
    {
        if (!IsEnabled(options, DdsLogLevel.Debug)) return;
        try
        {
            Debug(options, $"{direction} 토픽={topic.Name}, 타입={sample.GetType().FullName}{Environment.NewLine}{sample}");
        }
        catch (Exception) { /* 사용자 타입의 ToString 실패도 통신을 방해하지 않는다. */ }
    }
}
