# Toxon.Swim

> A .NET implemention of SWIM

This is an implementation of [SWIM](https://www.cs.cornell.edu/projects/Quicksilver/public_pdfs/SWIM.pdf) for .NET languages.
It is compatible with netstandard2.0+

**NOTE: this is untested and not production ready**

## Usage

Creating a client:

```csharp
var client = new SwimClient(
  new SwimHost(new SwimHost(new IPEndPoint(IPAddress.Loopback, 18210))),
  new SwimMeta(new Dictionary<string, string> { { "id", "18210" } })
  new SwimClientOptions()
);
```

Start listening for incoming messages:

```csharp
await client.StartAsync();
```

Join an existing cluster:

```csharp
// this list of hosts could be obtained through service discovery / multicast / etc...
// there is nothing special about them other than they are well-known members
await client.JoinAsync(new [] {
  new SwimHost(new SwimHost(new IPEndPoint(IPAddress.Loopback, 18200))),
  new SwimHost(new SwimHost(new IPEndPoint(IPAddress.Loopback, 18201)))
});
```

Getting the list of current cluster members:

```csharp
var members = client.Members.GetAll();
```

Subscribe to membership events:

```csharp
client.Members.OnJoined += (_, args) => _log.Information("Host {host} joined", args.Member.Host);
client.Members.OnUpdated += (_, args) => _log.Information("Host {host} updated", args.Member.Host);
client.Members.OnLeft += (_, args) => _log.Information("Host {host} left", args.Member.Host);
```

## Thing to do still

- Add unit tests
- Add integration tests
- Run a large scale benchmark / stability test
