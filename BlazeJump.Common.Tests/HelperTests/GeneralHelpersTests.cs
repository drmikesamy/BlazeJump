using System;
using static BlazeJump.Common.Helpers.GeneralHelpers;
using NUnit.Framework;

namespace BlazeJump.Common.Tests.HelperTests
{
    [TestFixture]
    public class GeneralHelpersTests
    {
        [TestCase(0, "1970-01-01 00:00:00")]
        [TestCase(1638489600, "2021-12-03 00:00:00")]
        [TestCase(1622502000, "2021-05-31 23:00:00")]
        public void UnixTimeStampToDateTime_ValidInput_ReturnsCorrectDateTime(long unixTimeStamp, string expectedDate)
        {
            // Act
            DateTime result = UnixTimeStampToDateTime(unixTimeStamp);
            
            // Assert
            DateTime expectedDateTime = DateTime.Parse(expectedDate);
            Assert.AreEqual(expectedDateTime, result);
        }

        [TestCase("1970-01-01 00:00:00", 0)]
        [TestCase("2021-12-03 00:00:00", 1638489600)]
        [TestCase("2021-06-01 00:00:00", 1622502000)]
        public void DateTimeToUnixTimeStamp_ValidInput_ReturnsCorrectUnixTimeStamp(string dateTimeStr, long expectedUnixTime)
        {
            // Arrange
            DateTime dateTime = DateTime.Parse(dateTimeStr);
            
            // Act
            long result = DateTimeToUnixTimeStamp(dateTime);
            
            // Assert
            Assert.AreEqual(expectedUnixTime, result);
        }

        [TestCase("82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2", "npub1sg6plzptd64u62a878hep2kev88swjh3tw00gjsfl8f237lmu63q0uf63m")]
        [TestCase("32e1827635450ebb3c5a7d12c1f8e7b2b514439ac10a67eef3d9fd9c5c68e245", "npub1xtscya34g58tk0z605fvr788k263gsu6cy9x0mhnm87echrgufzsevkk5s")]
        public void HexToNpub_ValidInput_ReturnsEncodedNpub(string hexString, string expectedNpub)
        {
            // Act
            string result = HexToNpub(hexString);
            
            // Assert
            Assert.AreEqual(expectedNpub, result);
        }

        [TestCase("npub1sg6plzptd64u62a878hep2kev88swjh3tw00gjsfl8f237lmu63q0uf63m", "82341f882b6eabcd2ba7f1ef90aad961cf074af15b9ef44a09f9d2a8fbfbe6a2")]
        [TestCase("npub1xtscya34g58tk0z605fvr788k263gsu6cy9x0mhnm87echrgufzsevkk5s", "32e1827635450ebb3c5a7d12c1f8e7b2b514439ac10a67eef3d9fd9c5c68e245")]
        public void NpubToHex_ValidInput_ReturnsDecodedHex(string npubString, string expectedHex)
        {
            // Act
            string result = NpubToHex(npubString);
            
            // Assert
            Assert.AreEqual(expectedHex, result);
        }

        [TestCase("3f4ed57d4846dd12dc4a97fef7fe1a85f2e0c5aea8d8df5de9c6afd625000c2b", "nsec18a8d2l2ggmw39hz2jll00ls6shewp3dw4rvd7h0fc6havfgqps4s4a94vw")]
        [TestCase("4efa539d2ef106f22babaf33ab77b1839cb86c18a4e384302ba2c2a4ac1a11bf", "nsec1fma988fw7yr0y2at4ue6kaa3swwtsmqc5n3cgvpt5tp2ftq6zxlsak3fq4")]
        public void HexToNsec_ValidInput_ReturnsEncodedNsec(string hexString, string expectedNsec)
        {
            // Act
            string result = HexToNsec(hexString);
            
            // Assert
            Assert.AreEqual(expectedNsec, result);
        }

        [TestCase("nsec1ds67xd9sdzy752g677e7l3956l06guafszpzwczxheeusu3wvhzqmyst2g", "6c35e334b06889ea291af7b3efc4b4d7dfa473a98082276046be73c8722e65c4")]
        [TestCase("nsec1cwh2fa50674vq4yvdejuemrcvltlxfqse2vs5f2vrv792x4yg23s8cfuqu", "c3aea4f68fd7aac0548c6e65ccec7867d7f32410ca990a254c1b3c551aa442a3")]
        public void NsecToHex_ValidInput_ReturnsDecodedHex(string nsecString, string expectedHex)
        {
            // Act
            string result = NsecToHex(nsecString);
            
            // Assert
            Assert.AreEqual(expectedHex, result);
        }

        [Test]
        public void NpubToHex_InvalidNpub_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => NpubToHex("npub1invalidstring"));
            Assert.That(ex.Message, Is.EqualTo("Invalid npub string"));
        }

        [Test]
        public void NsecToHex_InvalidNsec_ThrowsException()
        {
            // Act & Assert
            var ex = Assert.Throws<Exception>(() => NsecToHex("nsec1invalidstring"));
            Assert.That(ex.Message, Is.EqualTo("Invalid nsec string"));
        }
    }
}