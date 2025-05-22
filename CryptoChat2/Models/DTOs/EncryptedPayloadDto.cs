using System.ComponentModel.DataAnnotations;

namespace CryptoChat2.Models.DTOs;


    public class EncryptedPayloadDto
    {
        public int ReceiverId { get; set; }

        [Required]
        public string CipherText { get; set; } = string.Empty;

        [Required]
        public string Nonce { get; set; } = string.Empty;
    }
