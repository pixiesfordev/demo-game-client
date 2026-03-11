using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DOTween.dll",
		"UniTask.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<EnemyRole.<RunAI>d__26>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<PlayerRole.<DashAsync>d__47>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<PlayerRole.<RecoverStaminaLoop>d__48>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<Role.<RegenHPLoop>d__37>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<EnemyRole.<RunAI>d__26>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<PlayerRole.<DashAsync>d__47>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<PlayerRole.<RecoverStaminaLoop>d__48>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<Role.<RegenHPLoop>d__37>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.ComparisonComparer<object>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<object>
	// System.Func<int>
	// System.Predicate<object>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,EnemyRole.<RunAI>d__26>(Cysharp.Threading.Tasks.UniTask.Awaiter&,EnemyRole.<RunAI>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,PlayerRole.<DashAsync>d__47>(Cysharp.Threading.Tasks.UniTask.Awaiter&,PlayerRole.<DashAsync>d__47&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,PlayerRole.<RecoverStaminaLoop>d__48>(Cysharp.Threading.Tasks.UniTask.Awaiter&,PlayerRole.<RecoverStaminaLoop>d__48&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Role.<RegenHPLoop>d__37>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Role.<RegenHPLoop>d__37&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<EnemyRole.<RunAI>d__26>(EnemyRole.<RunAI>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<PlayerRole.<DashAsync>d__47>(PlayerRole.<DashAsync>d__47&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<PlayerRole.<RecoverStaminaLoop>d__48>(PlayerRole.<RecoverStaminaLoop>d__48&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<Role.<RegenHPLoop>d__37>(Role.<RegenHPLoop>d__37&)
		// object DG.Tweening.TweenSettingsExtensions.SetEase<object>(object,DG.Tweening.Ease)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
	}
}