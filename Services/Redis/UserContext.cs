using LatokenBot.Services.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LatokenBot.Services.Redis;
public class UserContext
{
    public ParsedRequest? ParsedRequest { get; set; }
    public ChatHistory? ChatHistory { get; set; }
}

