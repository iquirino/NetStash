# NetStash
Logstash sender for .NET

Send events to logstash instance via TCP

Saves all events into a sqlite database to prevent loss from network issues

Automatic synchronization when network connection is stablished

## Installation

Nugget Package: https://www.nuget.org/packages/NetStash

```
PM > Install-Package NetStash
```

## Usage

```
NetStashLog log = new NetStashLog("myhostname", 1233, "NSTest", "NSTestLog");

Dictionary<string, string> vals = new Dictionary<string, string>();
//Additional fields
vals.Add("customerid", "1235");

log.Error("Testing", vals);
```

## TLS Certificates

Note: certificateValidation = null >> Certificate validation will be discarded

```
NetStashLog log = new NetStashLog("myhostname", 1233, "NSTest", "NSTestLog", SslProtocols.Tls12, certificates, "domain.certificate.com", null);

Dictionary<string, string> vals = new Dictionary<string, string>();
//Additional fields
vals.Add("customerid", "1235");

log.Error("Testing", vals);
```

## Logstash config

```
input {
  tcp {
    port => 1233
    host => "192.168.0.151"
    codec => json
  }
}
filter {
  mutate { gsub => ["message", "@($NL$)@", "\r\n"] }
}
output {
  elasticsearch {

  }
}

```

## When to use

This project will work with ALL versions of elasticsearch.

If you just want to send log data, i recommend you to use NetStash.

If you are using elasticsearch 7+ and need to log error details:
I strongly recommend you to use https://github.com/elastic/apm-agent-dotnet Apm Agent Dotnet.
(I'm a contributor of this too)

## Who is using

Iron Mountain Brasil
 - All internals logs systems.
