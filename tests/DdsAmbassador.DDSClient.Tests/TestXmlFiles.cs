namespace DdsAmbassador.DDSClient.Tests;

internal sealed class TestXmlFiles : IDisposable
{
    private readonly string _directory;

    private TestXmlFiles(string directory)
    {
        _directory = directory;
        TopicsXmlPath = Path.Combine(directory, "topics.xml");
        QosProfilesXmlPath = Path.Combine(directory, "qos_profiles.xml");
        DdsSimXmlPath = Path.Combine(directory, "DDSSim.xml");
        DdsClientXmlPath = Path.Combine(directory, "dds_client.xml");
    }

    public string DirectoryPath => _directory;

    public string TopicsXmlPath { get; }

    public string QosProfilesXmlPath { get; }

    public string DdsSimXmlPath { get; }

    public string DdsClientXmlPath { get; }

    public static TestXmlFiles Create(
        string? topicsXml = null,
        string? qosProfilesXml = null,
        string? ddsSimXml = null,
        string? ddsClientXml = null)
    {
        var directory = Path.Combine(Path.GetTempPath(), "ddsclient-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        var files = new TestXmlFiles(directory);
        File.WriteAllText(files.TopicsXmlPath, topicsXml ?? DefaultTopicsXml);
        File.WriteAllText(files.QosProfilesXmlPath, qosProfilesXml ?? DefaultQosProfilesXml);
        File.WriteAllText(files.DdsSimXmlPath, ddsSimXml ?? DefaultDdsSimXml);
        File.WriteAllText(files.DdsClientXmlPath, ddsClientXml ?? $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <dds_client>
              <domain_id>7</domain_id>
              <transport>InMemory</transport>
              <topics_xml_path>{Path.GetFileName(files.TopicsXmlPath)}</topics_xml_path>
              <qos_profiles_xml_path>{Path.GetFileName(files.QosProfilesXmlPath)}</qos_profiles_xml_path>
              <dds_sim_xml_path>{Path.GetFileName(files.DdsSimXmlPath)}</dds_sim_xml_path>
            </dds_client>
            """);

        return files;
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private const string DefaultTopicsXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <topics>
          <topic name="AirThreatInformation" qos_profile="ReliableRealtime" direction="Both" />
        </topics>
        """;

    private const string DefaultQosProfilesXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <qos_library name="AmbassadorProfiles">
          <qos_profile name="ReliableRealtime">
            <datawriter_qos />
            <datareader_qos />
          </qos_profile>
        </qos_library>
        """;

    private const string DefaultDdsSimXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <dds>
          <types>
            <module name="MSG">
              <struct name="AirThreatInformation" />
            </module>
          </types>
        </dds>
        """;
}
