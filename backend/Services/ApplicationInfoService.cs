using backend.Models;

namespace backend.Services;

public class ApplicationInfoService : IApplicationInfoService
{
    private readonly IWebHostEnvironment _environment;

    public ApplicationInfoService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public ApplicationInfo GetInfo()
    {
        return new ApplicationInfo(
            "Информационная система управления вывозом лабораторных отходов",
            _environment.EnvironmentName,
            "ЛР10");
    }
}
