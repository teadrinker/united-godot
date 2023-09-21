//  ------ United Godot ------ 
//  teadrinker / Martin Eklund
//  License: MIT 

#if! UNITY_2017_1_OR_NEWER

using Godot;
using System.Collections.Generic;
 
namespace UnitedGodot
{
	public partial class UGGameObject : Node3D {

		private readonly static Dictionary<Node3D, UnityEngine.GameObject> _nodeToGameObject = new Dictionary<Node3D, UnityEngine.GameObject>();
		public static UnityEngine.GameObject FromNode3D(Node3D node3D) {
			if(node3D is UGGameObject @object)
			{
				if(@object.InnerGameObject == null)
					UnityEngine.Debug.LogError("UnitedGodot.GameObject.FromNode3D: InnerGameObject is null!");

				return @object.InnerGameObject;
			}
			else
			{
				if(_nodeToGameObject.TryGetValue(node3D, out var go))
					return go;

				go = new UnityEngine.GameObject(null, node3D);
				_nodeToGameObject.Add(node3D, go);
				return go;
			}
		}
		//public override void _Ready()
		//{
			//base._Ready(); // needed?

			// NOTE: It is too late to init here, child nodes will ask for InnerGameObject *before* _Ready() is called.
			//if(InnerGameObject == null) {
			//	InnerGameObject = new UnityEngine.GameObject(null, this);
			//}
		//}

		public UnityEngine.GameObject InnerGameObject { get { if(_innerGameObject == null) { _innerGameObject = new UnityEngine.GameObject(null, this); } return _innerGameObject; }  }
		private UnityEngine.GameObject _innerGameObject;
	}
}



#endif
