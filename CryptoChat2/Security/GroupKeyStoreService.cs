/*using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CryptoChat2.Security
{
    public class GroupKeyStoreService
    {
        private readonly ConcurrentDictionary<(int groupId, int userId), ECDiffieHellman> _keys
            = new();

        public byte[] GetPublicKey(int groupId, int userId)
        {
            var key = _keys.GetOrAdd((groupId, userId), _ => ECDiffieHellman.Create());
            return key.PublicKey.ExportSubjectPublicKeyInfo();
        }

        public byte[] DeriveSharedKey(int groupId, int senderId, byte[] recipientPublicKey)
        {
            var senderKey = _keys.GetOrAdd((groupId, senderId), _ => ECDiffieHellman.Create());
            using var recipientEcdh = ECDiffieHellman.Create();
            recipientEcdh.ImportSubjectPublicKeyInfo(recipientPublicKey, out _);
            return senderKey.DeriveKeyMaterial(recipientEcdh.PublicKey);
        }

        public ECDiffieHellman GetOrCreatePrivateKey(int groupId, int userId)
            => _keys.GetOrAdd((groupId, userId), _ => ECDiffieHellman.Create());
        
        public Task<byte[]> GetPublicKeyAsync(int groupId, int userId)
            => Task.FromResult(GetPublicKey(groupId, userId));

        public Task<byte[]> DeriveSharedKeyAsync(int groupId, int senderId, int receiverId)
        {
            var recipientPubKey = GetPublicKey(groupId, receiverId);
            var sharedKey = DeriveSharedKey(groupId, senderId, recipientPubKey);
            return Task.FromResult(sharedKey);
        }
        
        public void SetPublicKey(int groupId, int userId, byte[] publicKey)
        {
            var key = _keys.GetOrAdd((groupId, userId), _ => ECDiffieHellman.Create());
            key.ImportSubjectPublicKeyInfo(publicKey, out _);
        }

    }
} */