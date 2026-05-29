namespace Steamstatus.Configuration;

public class ServiceEndpointOptions
{
    public string Name { get; set; } =  string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<int> ExpectedStatusCodes { get; set; } = [];
}