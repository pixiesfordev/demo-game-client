---
apply: always
---

# Code Optimization & Refactoring Rules

1. **Minimal Changes**: Only modify code that is strictly necessary for the optimization. Do not change existing logic or style unless required.
2. **Consistent Naming**: Follow the existing naming conventions (e.g., maintain usage of underscores `_variable` if present in the context).
3. **Preserve Comments**: DO NOT delete existing comments unless they are factually incorrect. Keep original comments intact.
4. **Professional Documentation**:
   - Write comments as if for a final release version.
   - DO NOT use prefixes like "Optimization:", "Modified:", "Note:", or "Explanation:".
   - Avoid tutorial-style explanations in the code.
5. **Critical Error Reporting**: If you identify critical logic errors, explain them in the chat response text, NOT within the code comments.
6. **Flatten Nesting**: When encountering deeply nested loops (especially `for` loops), prioritize using `early return` or `continue` to reduce complexity and indentation depth.
7. **Async Preference**: Prioritize using `UniTask` (`Cysharp.Threading.Tasks`) over Unity Coroutines (`IEnumerator`) or standard .NET `Task` for asynchronous operations.
8. **Animation Preference**: Prioritize using `DOTween` (DG.Tweening) for procedural animations and interpolations over manual `Update` logic or legacy animation systems.