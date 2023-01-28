using System;

[Flags]
public enum NetworkType
{
    Offline = 1,
    Client  = 2,
    Server  = 4,
    Host    = Client | Server,
    Any     = Offline | Client | Server
}