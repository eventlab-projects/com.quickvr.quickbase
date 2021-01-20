using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Dictionary that is Unity serializable.
/// Simply two lists. Missing some common methods.
/// </summary>
/// <typeparam name="Key"></typeparam>
/// <typeparam name="Value"></typeparam>
/// Reference: http://forum.unity3d.com/threads/best-practices-for-generic-dictionary-serialization.215477/
[System.Serializable]
public class QuickUnityDictionary<Key, Value> {

	#region PROTECTED PARAMETERS

	[SerializeField] protected List<Key> _keys = new List<Key>();
	[SerializeField] protected List<Value> _values = new List<Value>();

	#endregion

	#region GET AND SET
	
	public void Add(Key key, Value value) {
		if (_keys.Contains(key)) return;
		
		_keys.Add(key);
		_values.Add(value);
	}
	
	public void Remove(Key key) {
		if (!_keys.Contains(key)) return;
		
		int index = _keys.IndexOf(key);
		
		_keys.RemoveAt(index);
		_values.RemoveAt(index);
	}
	
	public bool TryGetValue(Key key, out Value value) {
		if (_keys.Count != _values.Count) {
			_keys.Clear();
			_values.Clear();
			value = default(Value);
			return false;
		}
		
		if (!_keys.Contains(key)) {
			value = default(Value);
			return false;
		}
		
		int index = _keys.IndexOf(key);
		value = _values[index];
		
		return true;
	}
	
	public void ChangeValue(Key key, Value value) {
		if (!_keys.Contains(key)) return;
		
		int index = _keys.IndexOf(key);
		_values[index] = value;
	}

	public virtual int Count() {
		return _keys.Count;
	}

	public virtual bool ContainsKey(Key key) {
		return _keys.Contains(key);
	}

	#endregion

}