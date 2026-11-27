namespace DdsAmbassador.DDSClient;

/// <summary>명시적인 파일 경로를 우선 적용하고 개발·배포 실행 위치의 차이를 흡수하는 경로 탐색을 수행한다.</summary>
internal static class DdsClientOptionResolver
{
    public static string ResolveQosProfilesXmlPath(DdsClientOptions options)
    {
        return ResolveRequiredFile(
            options.QosProfilesXmlPath,
            Path.Combine("definitions", "qos_profiles.xml"),
            nameof(options.QosProfilesXmlPath));
    }

    private static string ResolveRequiredFile(
        string? optionValue,
        string defaultRelativePath,
        string optionName)
    {
        if (!string.IsNullOrWhiteSpace(optionValue))
        {
            return RequireFile(optionValue, optionName);
        }

        // 명시 경로가 잘못되었다면 위에서 즉시 실패한다. 기본 파일로 조용히 대체하면 다른 망의 QoS를 읽을 수 있다.
        foreach (var baseDirectory in EnumerateBaseDirectories())
        {
            foreach (var relativePath in new[] { defaultRelativePath, Path.Combine("share", "dds_cpp", defaultRelativePath) })
            {
                var candidate = Path.Combine(baseDirectory, relativePath);
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
            }
        }

        throw new DdsConfigurationException(
            $"{optionName}이(가) 필요합니다. 경로를 지정하거나 {defaultRelativePath} 파일을 현재 폴더 또는 실행 폴더에 두세요.");
    }

    private static string RequireFile(string path, string optionName)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new DdsConfigurationException($"{optionName} 파일이 없습니다: {fullPath}");
        }

        return fullPath;
    }

    private static IEnumerable<string> EnumerateBaseDirectories()
    {
        // 현재 작업 폴더를 먼저 확인하고 실행 파일 폴더부터 상위로 올라간다.
        // 같은 경로의 중복 접근을 피하고 탐색 깊이를 제한하여 루트까지 무제한 탐색하지 않는다.
        var seen = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var current = Path.GetFullPath(Environment.CurrentDirectory);
        seen.Add(current);
        yield return current;
        var directory = AppContext.BaseDirectory;
        for (var depth = 0; directory is not null && depth < 16; depth++)
        {
            var fullPath = Path.GetFullPath(directory);
            if (seen.Add(fullPath)) yield return fullPath;
            directory = new DirectoryInfo(directory).Parent?.FullName;
        }
    }

}
