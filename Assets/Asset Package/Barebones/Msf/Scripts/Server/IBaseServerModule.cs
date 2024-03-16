using System;
using System.Collections.Generic;

namespace Barebones.MasterServer
{
    public interface IBaseServerModule
    {
        List<Type> Dependencies { get; }
        List<Type> OptionalDependencies { get; }
        ServerBehaviour Server { get; set; }
        void Initialize(IServer server);
    }
}