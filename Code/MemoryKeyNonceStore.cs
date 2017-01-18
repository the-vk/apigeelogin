using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetOpenAuth.Messaging.Bindings;

namespace ApigeeLogin.Code
{
	public class MemoryKeyNonceStore : INonceStore, ICryptoKeyStore
	{
		private Dictionary<String, Dictionary<String, CryptoKey>> _store =
			new Dictionary<String, Dictionary<String, CryptoKey>>();

		public bool StoreNonce(string context, string nonce, DateTime timestampUtc)
		{
			return true;
		}

		public CryptoKey GetKey(string bucket, string handle)
		{
			if (!_store.ContainsKey(bucket) || !_store[bucket].ContainsKey(handle))
			{
				return null;
			}
			return _store[bucket][handle];
		}

		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
		{
			if (!_store.ContainsKey(bucket)) return Enumerable.Empty<KeyValuePair<string, CryptoKey>>();
			return _store[bucket];
		}

		public void StoreKey(string bucket, string handle, CryptoKey key)
		{
			if (!_store.ContainsKey(bucket))
			{
				_store.Add(bucket, new Dictionary<string, CryptoKey>());
			}
			_store[bucket][handle] = key;
		}

		public void RemoveKey(string bucket, string handle)
		{
			if (!_store.ContainsKey(bucket))
			{
				return;
			}
			if (!_store[bucket].ContainsKey(handle))
			{
				return;
			}
			_store[bucket].Remove(handle);
		}
	}
}