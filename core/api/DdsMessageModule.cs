using System.Collections.ObjectModel;

namespace DdsAmbassador.DDSClient;

/// <summary>사용할 생성 타입을 명시적으로 선택하는 불변 메시지 모듈이다.</summary>
public sealed class DdsMessageModule
{
    /// <summary>이름·버전과 타입 등록부로 모듈을 만든다. 등록부는 복사하여 외부 변경과 분리한다.</summary>
    public DdsMessageModule(string name, string version, IReadOnlyDictionary<string, Type> types)
        : this(name, version, types, null) { }

    /// <summary>생성 XML 지문을 포함한 모듈을 만든다. 생성기가 사용하며 지문은 DDS 메시지 데이터에 포함되지 않는다.</summary>
    public DdsMessageModule(string name, string version, IReadOnlyDictionary<string, Type> types, string? schemaSha256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(types);
        if (types.Any(pair => string.IsNullOrWhiteSpace(pair.Key) || pair.Value is null || pair.Value == typeof(void)))
            throw new DdsConfigurationException("모듈 항목에는 이름과 생성된 타입이 필요합니다.");
        Name = name;
        Version = version;
        SchemaSha256 = schemaSha256;
        Types = new ReadOnlyDictionary<string, Type>(new Dictionary<string, Type>(types, StringComparer.Ordinal));
    }
    /// <summary>같은 연결 안에서 고유해야 하는 논리적 모듈 이름이다.</summary>
    public string Name { get; }
    /// <summary>모듈이 제공하는 버전 정보다. 이 값만으로 상대편과의 DDS 타입 호환성이 보장되지는 않는다.</summary>
    public string Version { get; }
    /// <summary>XML과 컴파일된 모듈의 일치 확인에 사용하는 선택적 지문이다.</summary>
    public string? SchemaSha256 { get; }
    /// <summary>짧은 메시지 이름과 생성된 CLR 타입의 읽기 전용 대응표다.</summary>
    public IReadOnlyDictionary<string, Type> Types { get; }
}

// 연결에 선택된 모듈만 합친다. 타입 이름이 같다는 이유로 서로 다른 XML의 CLR 타입을 하나로 취급하지 않는다.
internal static class MessageCatalog
{
    public static IReadOnlyList<KeyValuePair<string, Type>> Read(DdsClientOptions options)
    {
        var modules = options.MessageModules ?? throw new DdsConfigurationException("MessageModules는 null일 수 없습니다.");
        var names = new HashSet<string>(StringComparer.Ordinal);
        var types = new HashSet<Type>();
        var entries = new List<KeyValuePair<string, Type>>();
        foreach (var module in modules)
        {
            if (module is null || !names.Add(module.Name))
                throw new DdsConfigurationException("같은 연결의 메시지 모듈 이름은 중복될 수 없습니다.");
            foreach (var entry in module.Types)
            {
                if (!types.Add(entry.Value))
                    throw new DdsConfigurationException($"Type '{entry.Value}' 이(가) 중복 등록되었습니다.");
                entries.Add(entry);
            }
        }
        return entries;
    }
}
