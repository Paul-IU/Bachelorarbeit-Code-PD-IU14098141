using System.Runtime.CompilerServices;

// Erlaubt den Tests direkten Zugriff auf internal-Elemente (z.B.
// Unit.ApplyDamage), um Invarianten wie das HP-Clamping isoliert zu
// testen, ohne die öffentliche API dafür aufweichen zu müssen.
[assembly: InternalsVisibleTo("TacticsCore.Tests")]
