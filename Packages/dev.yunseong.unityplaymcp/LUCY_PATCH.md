# Lucy integration patch

Embedded Unity Play MCP v0.4.0 (upstream Git revision d876428212, MIT).

`AffordanceILPostProcessor.WillProcess` returns false in this project. The optional IL affordance discovery hit a fatal stack overflow compiling Assembly-CSharp under Unity 2022.3.34f1. The stack cycles through IlReading.Describe → Arithmetic → Read → WhichScene → Describe, resetting depth as it goes. Catching Exception cannot contain this process-level failure.

The independent input weaving processor, scene readings, screenshots, and basic input controls remain available. Rich generated action discovery is unavailable. Remove this one-method patch only after an upstream depth-bounded implementation passes compilation here. The upstream package source and license are otherwise preserved.
