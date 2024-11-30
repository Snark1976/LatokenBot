using Microsoft.SemanticKernel;

namespace LatokenBot.Services.AI;
public class KernelProvider
{
    public KernelProvider(IConfiguration configuration)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            "gpt-4-1106-preview",
            configuration["OpenAI:ApiKey"]!
        );
        Kernel = builder.Build();
    }

    public Kernel Kernel { get; init; }
}