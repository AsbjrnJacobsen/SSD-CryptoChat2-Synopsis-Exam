using System.Collections.Generic;

namespace CryptoChat2.ClientLib.Models.DTOs;

public class EncryptedGroupMessageDto
{
    public List<EncryptedPayloadDto> Payloads { get; set; } = new();
}