using System.Linq.Expressions;

namespace DdsAmbassador.DDSClient.Transport;

/// <summary>RTI 생성 타입의 복사 생성자를 타입별로 한 번 찾아 대여 샘플의 독립 복사본을 만든다.</summary>
/// <remarks>매 메시지마다 리플렉션을 실행하지 않도록 컴파일한 델리게이트를 캐시한다.
/// 일반적인 얕은 복사로 대체하면 시퀀스 등 내부 버퍼의 수명을 보장할 수 없으므로 생성된 복사 생성자가 필요하다.</remarks>
internal static class GeneratedSampleCloner<T>
{
    internal static readonly Func<T, T> Copy = CreateCloneDelegate();

    private static Func<T, T> CreateCloneDelegate()
    {
        var copyConstructor = typeof(T).GetConstructor([typeof(T)])
            ?? throw new DdsConfigurationException($"{typeof(T)}에 생성된 복사 생성자가 없습니다.");
        var sampleParameter = Expression.Parameter(typeof(T), "sample");
        return Expression.Lambda<Func<T, T>>(Expression.New(copyConstructor, sampleParameter), sampleParameter).Compile();
    }
}
