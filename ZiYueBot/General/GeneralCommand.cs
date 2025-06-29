using System.Collections;
using ZiYueBot.Core;

namespace ZiYueBot.General;

public abstract class GeneralCommand : Command
{
    public abstract Platform SupportedPlatform { get; }
    
    public abstract IEnumerable Invoke(Platform platform, EventType eventType, string userName, ulong userId, string[] args);
}