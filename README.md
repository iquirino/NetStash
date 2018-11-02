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

## Who is using

Iron Mountain Brasil
 - All internals logs systems.
