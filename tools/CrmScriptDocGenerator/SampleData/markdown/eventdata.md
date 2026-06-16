---
uid: crmscript_eventdata
title: EventData
---

# EventData

EventData gives you access to contextual information in the current execution context.

## Example

```crmscript
EventData ed = getEventData();
String value = ed.getInputValue("ticketId");
```

See [xref:CRMScript.Native.EventData.getInputValue(String)](xref:CRMScript.Native.EventData.getInputValue%28String%29)

