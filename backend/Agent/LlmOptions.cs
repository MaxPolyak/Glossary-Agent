namespace GlossaryAgent.Agent;

public class LlmOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1235/v1";

    public string Model { get; set; } = "qwen/qwen3.8-27b";

    public string ApiKey { get; set; } = "lm-studio";

    public int MaxChunkChars { get; set; } = 6000;
}
