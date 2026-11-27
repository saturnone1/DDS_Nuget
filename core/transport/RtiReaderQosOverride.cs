using System.Net;
using Rti.Dds.Core;
using Rti.Dds.Subscription;

namespace DdsAmbassador.DDSClient.Transport;

// 원본 XML 경로는 유지하고 선택된 Reader의 해석된 QoS에만 변경을 적용한다.
/// <summary>이 연결에서 만든 Reader에만 멀티캐스트 수신 주소 덮어쓰기를 적용한다.</summary>
/// <remarks>QoS XML을 수정하거나 임시 파일을 만들지 않는다. 프로파일 상속을 RTI가 해석한 뒤
/// 단일 수신 주소 항목만 바꾸므로 다른 연결이나 Writer의 QoS에는 영향을 주지 않는다.</remarks>
internal sealed class RtiReaderQosOverride : IDisposable
{
    private string? _multicastAddress;
    public string Resolve(string qosProfilesXmlPath, string? multicastAddress = null)
    {
        // C++의 빈 주소와 같은 의미로 처리한다. 미지정 주소는 원래 QoS를 유지한다.
        _multicastAddress = string.IsNullOrEmpty(multicastAddress) ? null : multicastAddress;
        if (_multicastAddress is null)
        {
            return qosProfilesXmlPath;
        }

        ValidateMulticastAddress(_multicastAddress);
        return qosProfilesXmlPath;
    }

    public DataReaderQos ReaderQos(QosProvider provider, string profile)
    {
        var qos = provider.GetDataReaderQos(profile);
        if (_multicastAddress is null) return qos;
        // 여러 멀티캐스트 항목 중 어느 것을 바꿀지 임의로 추측하지 않는다. 복수 주소 구성은 XML에서 명시한다.
        if (qos.Multicast.Value.Count != 1)
            throw new DdsConfigurationException($"Profile '{profile}' 의 멀티캐스트 수신 설정이 하나여야 덮어쓸 수 있습니다. 여러 주소는 QoS XML에 직접 설정하세요.");
        return qos.WithMulticast(policy => {
            policy.Value[0] = policy.Value[0].With(setting => setting.ReceiveAddress = _multicastAddress);
        });
    }

    private static void ValidateMulticastAddress(string value)
    {
        if (!IPAddress.TryParse(value, out var address) ||
            address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            throw new DdsConfigurationException($"'{value}'은(는) IPv4 멀티캐스트 주소여야 합니다.");
        }

        var firstOctet = address.GetAddressBytes()[0];
        if (firstOctet is < 224 or > 239)
        {
            throw new DdsConfigurationException($"'{value}'은(는) 224.0.0.0/4 범위의 IPv4 멀티캐스트 주소여야 합니다.");
        }
    }

    public void Dispose()
    {
        // 임시 파일이나 전역 상태를 만들지 않는다.
    }

}
